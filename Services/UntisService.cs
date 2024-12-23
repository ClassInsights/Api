using System.Globalization;
using System.Net;
using System.Text.Json;
using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Api.Services;

public class UntisService(
    ILogger<UntisService> logger,
    SettingsService settingsService,
    IMapper mapper,
    IServiceScopeFactory serviceScope,
    IConfiguration configuration) : BackgroundService
{
    private readonly object _lock = new();
    private TokenDto? _tokenResponse;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("UntisService is running");
        using PeriodicTimer timer = new(TimeSpan.FromHours(5));
        try
        {
            do
            {
                await UpdateUntisRecords();
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

            TimetableDto? timetable;

            if (force || !await context.Rooms.AsNoTracking().AnyAsync())
                // get timetable for all rooms 
                timetable = await GetTimetableAsync();
            else
            {
                // get timetable for classinsights rooms
                timetable = await GetTimetableAsync(await context.Rooms.AsNoTracking().Where(x => x.Computers.Count > 0).Select(x => x.RoomId).ToArrayAsync());
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
                await context.Lessons.ExecuteDeleteAsync();

                List<Lesson> lessons = (from period in periods
                    where period.Status is PeriodStatusEnum.REGULAR or PeriodStatusEnum.ADDED
                    from classDto in period.Classes
                    from subjects in period.Subjects
                    from room in period.Rooms
                    select new Lesson
                    {
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

    private async Task UpdateMasterDataAsync(TimetableDto timetable, ClassInsightsContext context)
    {
        if (timetable.MasterData.Rooms is { Count: > 0 } rooms)
        {
            var dbRooms = mapper.Map<List<Room>>(rooms);
            await context.BulkInsertOrUpdateAsync(dbRooms);
        }

        if (timetable.MasterData.Subjects is { Count: > 0 } subjects)
        {
            var dbSubjects = mapper.Map<List<Subject>>(subjects);
            await context.BulkInsertOrUpdateAsync(dbSubjects);
        }

        if (timetable.MasterData.Classes is { Count: > 0 } classes)
        {
            var dbClasses = mapper.Map<List<Class>>(classes);
            await context.BulkInsertOrUpdateAsync(dbClasses);
        }
    }
    
    private async Task<TimetableDto> GetTimetableAsync(long[]? roomIds = null)
    {
        var response = await CallApiEndpointAsync("https://classinsights.at/api/untis/timetable" + (roomIds == null ? "" : $"?room={string.Join("&room=", roomIds)}"), HttpMethod.Get);

        response.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        return await response.Content.ReadFromJsonAsync<TimetableDto>(options) ??
               throw new NullReferenceException("Timetable response is null");
    }

    private async Task<HttpResponseMessage> CallApiEndpointAsync(string endpoint, HttpMethod method,
        HttpContent? content = null)
    {
        for (var i = 0; i < 3; i++)
        {
            using var client = new HttpClient();
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
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {license}");
        var response = await client.PostAsync("https://classinsights.at/api/login", null);

        response.EnsureSuccessStatusCode();

        var authToken = await response.Content.ReadFromJsonAsync<TokenDto>();
        if (authToken == null || string.IsNullOrEmpty(authToken.AccessToken))
            throw new NullReferenceException("Access token is null");

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        return authToken;
    }
}