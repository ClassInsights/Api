using Api.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class LogsController : ControllerBase
{
    private readonly ClassInsightsContext _context;

    /// <inheritdoc />
    public LogsController(ClassInsightsContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Find all Logs
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLogs()
    {
        return Ok(await _context.Logs.ToListAsync());
    }
}