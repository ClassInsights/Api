using Api.Models.Database;
using Api.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Api.Controllers;

[ApiController]
[Route("api/computers/{computerId}/logs")]
public class ComputerLogsController(ClassInsightsContext context) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Create a new computer log")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ComputerLogDto>(StatusCodes.Status201Created)]

    public async Task<IActionResult> PostLog(string computerId, [FromBody] ComputerLogDto log)
    {
        if (computerId != log.ComputerId.ToString())
            return BadRequest("ClientId mismatch.");

        var created = context.ComputerLogs.Add(log.ToComputerLog());
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLogs), new { computerId }, created.Entity.ToDto());
    }

    [HttpPost("batch")]
    [EndpointSummary("Create multiple new computer logs")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PostLogs(string computerId, [FromBody] List<ComputerLogDto> logs)
    {
        if (logs.Any(l => l.ComputerId.ToString() != computerId))
            return BadRequest("All logs must match the clientId in the route.");

        context.ComputerLogs.AddRange(logs.Select(l => l.ToComputerLog()));
        await context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    [EndpointSummary("Get all logs for a computer")]
    [ProducesResponseType<List<ComputerLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        string computerId,
        [FromQuery] Instant? from = null,
        [FromQuery] Instant? to = null,
        [FromQuery] string? level = null)
    {
        var query = context.ComputerLogs
            .Where(l => l.ComputerId.ToString() == computerId);

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);
        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Select(l => l.ToDto())
            .ToListAsync();

        return Ok(logs);
    }
}
