using Api.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LogsController(ClassInsightsContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Find all Logs")]
    [ProducesResponseType<List<Log>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs()
    {
        return Ok(await context.Logs.AsNoTracking().ToListAsync());
    }
}