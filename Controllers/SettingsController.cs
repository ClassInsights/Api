using System.ComponentModel;
using Api.Models.Dto;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SettingsController(SettingsService settingsService) : ControllerBase
{
    [HttpGet("dashboard")]
    [Authorize]
    [EndpointSummary("Find dashboard settings")]
    [ProducesResponseType<SettingsDto.Dashboard>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardSettings()
    {
        var settings = await settingsService.GetSettingAsync<SettingsDto.Dashboard>("dashboard");
        if (settings != null) return Ok(settings);

        settings = new SettingsDto.Dashboard();
        settings.AfkTimeout = Math.Max(5, settings.AfkTimeout);
        await settingsService.SetSettingAsync("dashboard", settings);

        return Ok(settings);
    }

    [HttpPut("dashboard")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Set dashboard settings")]
    public async Task<IActionResult> SetDashboardSettings(
        [Description("New values for the dashboard settings")]
        SettingsDto.Dashboard settings)
    {
        await settingsService.SetSettingAsync("dashboard", settings);
        return Ok();
    }
}