using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    /// <summary>
    ///     Update ClientCredentials for Azure Graph Authentication
    /// </summary>
    /// <param name="clientCredentials">New ClientCredentials</param>
    /// <returns></returns>
    [HttpPatch("graph/credentials")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateGraphSecret(List<Dictionary<string, string>> clientCredentials)
    {
        var rawConfig = await System.IO.File.ReadAllTextAsync("appsettings.json");
        var jsonConfig = JObject.Parse(rawConfig);

        jsonConfig["AzureAd"] ??= new JObject();
        jsonConfig["AzureAd"]!["ClientCredentials"] = JArray.FromObject(clientCredentials);

        await System.IO.File.WriteAllTextAsync("appsettings.json", jsonConfig.ToString(Formatting.Indented));
        return Ok();
    }
}