using Api.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class LogsController(ClassInsightsContext context) : ControllerBase
{
    /// <summary>
    ///     Find all Logs
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLogs()
    {
        return Ok(await context.Logs.AsNoTracking().ToListAsync());
    }
}