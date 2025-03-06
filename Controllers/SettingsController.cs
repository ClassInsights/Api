using Api.Models.Dto;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SettingsController(SettingsService settingsService) : ControllerBase
{
    [HttpGet("dashboard"), ProducesResponseType(StatusCodes.Status200OK), Authorize]
    public async Task<IActionResult> GetDashboardSettings()
    {
        var settings = await settingsService.GetSettingAsync<SettingsDto.Dashboard>("dashboard");
        if (settings != null) return Ok(settings);
        
        settings = new SettingsDto.Dashboard();
        await settingsService.SetSettingAsync("dashboard", settings);

        return Ok(settings);
    }

    [HttpPut("dashboard"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetDashboardSettings(SettingsDto.Dashboard settings)
    {
        await settingsService.SetSettingAsync("dashboard", settings);
        return Ok();
    }
}