using Api.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class SchoolYearController : ControllerBase
{
    /// <summary>
    ///     Update current SchoolYear
    /// </summary>
    /// <param name="schoolYear">New SchoolYear</param>
    /// <returns>SchoolYear</returns>
    [HttpPost]
    [IsLocal]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateSchoolYear(ApiModels.SchoolYear schoolYear)
    {
        var rawConfig = await System.IO.File.ReadAllTextAsync("appsettings.json");
        var jsonConfig = JObject.Parse(rawConfig);

        jsonConfig["Dashboard"] ??= new JObject();
        jsonConfig["Dashboard"]!["SchoolYear"] = JObject.FromObject(schoolYear);

        await System.IO.File.WriteAllTextAsync("appsettings.json", jsonConfig.ToString(Formatting.Indented));
        return Ok(schoolYear);
    }
}