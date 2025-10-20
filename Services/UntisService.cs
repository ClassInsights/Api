using System.Globalization;
using System.Net;
using System.Text.Json;
using Api.Models.Database;
using Api.Models.Dto;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using NodaTime.Text;

namespace Api.Services;

public class UntisService(
    ILogger<UntisService> logger,
    SettingsService settingsService,
    IClock clock,
    IHttpClientFactory httpClientFactory,
    IServiceScopeFactory serviceScope,
    IConfiguration configuration) : BackgroundService
{
    private readonly Lock _lock = new();
    private int _fetchCount;
    private Instant? _lastFetch;
    private TokenDto? _tokenResponse;
    private bool FetchAllLessons => _fetchCount % 10 == 0; // fetch all lessons every 10 fetches

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("UntisService is running");
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(30));
        try
        {
            do
            {
                var hour = clock.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).Hour;
                if (hour is < 6 or > 18)
                {
                    logger.LogInformation("Time is not between 6:00 and 18:00. Skipping update.");
                    _fetchCount = 0; // reset after a school day to ensure that the first iteration the next day (after 6:00) will fetch all lessons
                    continue;
                }
                
                await UpdateUntisRecords();
                _fetchCount++;
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("UntisService is stopping");
        }
    }

    public async Task UpdateUntisRecords(bool force = false)
    {
        try
        {
            await using var scope = serviceScope.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ClassInsightsContext>();

            UntisTimetable? timetable;

            if (force || !await context.Rooms.AsNoTracking().AnyAsync())
                // get timetable for all rooms 
            {
                timetable = await GetTimetableAsync();
            }
            else
            {
                // get timetable for classinsights rooms
                var roomIds = await context.Rooms.AsNoTracking().Where(x => x.Computers.Count > 0).Select(x => x.RoomId)
                    .ToArrayAsync();
                if (roomIds.Length <= 0)
                    return; // don't request timetable if classinsights isn't installed anywhere
                timetable = await GetTimetableAsync(roomIds);
            }

            if (timetable.HeaderData is { SchoolYearEnd: { } endDate, SchoolYearStart: { } startDate })
            {
                await settingsService.SetSettingAsync("SchoolYearStart",
                    startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                await settingsService.SetSettingAsync("SchoolYearEnd",
                    endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }

            // update classes, rooms and subjects
            await UpdateMasterDataAsync(timetable, context);

            if (timetable.TimetableData.Periods is { Count: > 0 } periods)
            {
                var tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();

                // delete all lessons on full fetch
                if (FetchAllLessons)
                {
                    await context.Lessons.ExecuteDeleteAsync();
                }
                else
                {
                    var cancelledLessons = periods.Where(x => x.Status == UntisPeriodStatus.Cancelled).Select(x => x.Id)
                        .ToList();
                    if (cancelledLessons.Count > 0)
                        await context.Lessons.Where(lesson => cancelledLessons.Contains(lesson.PeriodId))
                            .ExecuteDeleteAsync();
                }

                List<Lesson> lessons = (from period in periods
                    where period.Status is not UntisPeriodStatus.Cancelled
                    from classDto in period.Classes
                    where classDto.Status is not UntisResourceStatus.Removed
                    from subjects in period.Subjects
                    where subjects.Status is not UntisResourceStatus.Removed
                    from room in period.Rooms
                    where room.Status is not UntisResourceStatus.Removed
                    select new Lesson
                    {
                        PeriodId = period.Id,
                        ClassId = classDto.Id,
                        SubjectId = subjects.Id,
                        RoomId = room.Id,
                        Start = period.Start.InZoneLeniently(tz).ToInstant(),
                        End = period.End.InZoneLeniently(tz).ToInstant()
                    }).Distinct().ToList();

                context.Lessons.AddRange(lessons);
            }

            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while authenticating.");
        }
    }

    private async Task UpdateMasterDataAsync(UntisTimetable untisTimetable, ClassInsightsContext context)
    {
        if (untisTimetable.MasterData.Rooms is { Count: > 0 } rooms)
        {
            var dbRooms = rooms.Select(x => new Room
            {
                RoomId = x.Id,
                DisplayName = x.DisplayName
            }).ToList();
            await context.BulkInsertOrUpdateAsync(dbRooms,
                config => { config.PropertiesToExclude = [nameof(Room.OrganizationUnit), nameof(Room.Enabled)]; });
        }

        if (untisTimetable.MasterData.Subjects is { Count: > 0 } subjects)
        {
            var dbSubjects = subjects.Select(x => new Subject
            {
                SubjectId = x.Id,
                DisplayName = x.DisplayName
            }).ToList();
            await context.BulkInsertOrUpdateAsync(dbSubjects);
        }

        if (untisTimetable.MasterData.Classes is { Count: > 0 } classes)
        {
            var dbClasses = classes.Select(x => new Class
            {
                ClassId = x.Id,
                DisplayName = x.DisplayName
            }).ToList();
            await context.BulkInsertOrUpdateAsync(dbClasses,
                config => { config.PropertiesToExclude = [nameof(Class.AzureGroupId)]; });
        }
    }

    private async Task<UntisTimetable> GetTimetableAsync(long[]? roomIds = null)
    {
        var server = configuration["Server"]!;
        var response = await CallApiEndpointAsync(
            $"{server}/api/untis/timetable" + (roomIds == null
                ? ""
                : $"?room={string.Join("&room=", roomIds)}&lastModified={(FetchAllLessons ? "" : _lastFetch?.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).ToString(InstantPattern.ExtendedIso.PatternText, CultureInfo.InvariantCulture))}"),
            HttpMethod.Get);

        response.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        _lastFetch = clock.GetCurrentInstant();
        return await response.Content.ReadFromJsonAsync<UntisTimetable>(options) ??
               throw new NullReferenceException("Timetable response is null");
    }

    private async Task<HttpResponseMessage> CallApiEndpointAsync(string endpoint, HttpMethod method,
        HttpContent? content = null)
    {
        for (var i = 0; i < 3; i++)
        {
            using var client = httpClientFactory.CreateClient();
            var accessToken = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(method, endpoint) { Content = content };
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.Unauthorized) return response;
            logger.LogWarning("Access token expired. Retrying...");
            lock (_lock)
            {
                _tokenResponse = null; // Force re-authentication
            }
        }

        throw new HttpRequestException("Credentials invalid");
    }

    private async Task<string> GetAccessTokenAsync()
    {
        lock (_lock)
        {
            if (_tokenResponse != null) return _tokenResponse.AccessToken;
        }

        var newAuthResponse = await AuthenticateAsync();
        lock (_lock)
        {
            _tokenResponse = newAuthResponse;
        }

        return _tokenResponse.AccessToken;
    }

    private async Task<TokenDto> AuthenticateAsync()
    {
        if (configuration["License"] is not { } license)
            throw new InvalidOperationException("License is not configured");

        logger.LogInformation("Fetching new Access Token");
        using var client = httpClientFactory.CreateClient();
        var server = configuration["Server"]!;

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {license}");
        var response = await client.PostAsync($"{server}/api/login", null);

        response.EnsureSuccessStatusCode();

        var authToken = await response.Content.ReadFromJsonAsync<TokenDto>();
        if (authToken == null || string.IsNullOrEmpty(authToken.AccessToken))
            throw new NullReferenceException("Access token is null");

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        return authToken;
    }
}