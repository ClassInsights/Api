using System.Net;
using System.Net.Http.Headers;
using Api.Models.Dto;

namespace Api.Services;

public class IdentityService(IConfiguration config, ILogger<IdentityService> logger, IHostApplicationLifetime hostApplicationLifetime,  SettingsService settingsService): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromDays(3));
        do
        {
            try
            {
                await UpdateIdentity();
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Unauthorized)
                {
                    logger.LogCritical("Invalid or expired license! Please visit https://classinsights.at and contact us! ");
                    hostApplicationLifetime.StopApplication();
                }
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task UpdateIdentity()
    {
        if (config["License"] is not { } license)
            throw new InvalidOperationException("License is not configured");
        
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://classinsights.at/api/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", license);
        
        var loginResponse = await client.PostAsync("login", null);
        loginResponse.EnsureSuccessStatusCode();
        
        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens?.AccessToken);
        
        var schoolResponse = await client.GetAsync("school");
        schoolResponse.EnsureSuccessStatusCode();

        var school = await schoolResponse.Content.ReadFromJsonAsync<ServerDto.SchoolDto>();
        if (school is null)
            throw new InvalidOperationException("Your license is invalid");
        
        await settingsService.SetSettingAsync("SchoolName", school.Name);
        await settingsService.SetSettingAsync("SchoolId", school.SchoolId.ToString());
    }
}