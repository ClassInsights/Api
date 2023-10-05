using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class ConfigController : ControllerBase
{
    /// <summary>
    ///     Get Config of editable Properties
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetConfig()
    {
        var rawConfig = await System.IO.File.ReadAllTextAsync("appsettings.json");
        var jsonConfig = JObject.Parse(rawConfig);
        
        return Ok(jsonConfig["Dashboard"]?.ToString(Formatting.Indented));
    }

    /// <summary>
    ///     Overwrite Config with new Properties
    /// </summary>
    /// <param name="config">New Config Object</param>
    /// <returns></returns>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateConfig(dynamic config)
    {
        var rawConfig = await System.IO.File.ReadAllTextAsync("appsettings.json");
        var jsonConfig = JObject.Parse(rawConfig);

        jsonConfig["Dashboard"] = JObject.Parse(config.ToString());

        await System.IO.File.WriteAllTextAsync("appsettings.json", jsonConfig.ToString(Formatting.Indented));
        return Ok();
    }
}