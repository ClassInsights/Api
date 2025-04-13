using System.Net.WebSockets;
using System.Text;
using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class ComputersController(IClock clock, ClassInsightsContext context, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Adds or updates a computer
    /// </summary>
    /// <param name="computerDto">New computer</param>
    /// <returns>
    ///     <see cref="ApiDto.ComputerDto" />
    /// </returns>
    [HttpPost]
    [Authorize(Roles = "Computer")]
    public async Task<IActionResult> UpdateComputer(ApiDto.ComputerDto computerDto)
    {
        // map to database object to receive new ComputerId if it was created
        var dbComputer = mapper.Map<Computer>(computerDto);
        context.Update(dbComputer);
        await context.SaveChangesAsync();

        return Ok(mapper.Map<ApiDto.ComputerDto>(dbComputer));
    }
    
    /// <summary>
    ///     Find all Computers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetComputers()
    {
        var computers = await context.Computers.AsNoTracking().ToListAsync();
        return Ok(mapper.Map<List<ApiDto.ComputerDto>>(computers));
    }

    /// <summary>
    ///     Get information of computer by name
    /// </summary>
    /// <param name="name">Name of computer</param>
    /// <returns>
    ///     <see cref="Computer" />
    /// </returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetComputer(string name)
    {
        if (await context.Computers.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains(name)) is { } computer)
            return Ok(mapper.Map<ApiDto.ComputerDto>(computer));
        return NotFound();
    }

    /// <summary>
    ///     Send command to computer
    /// </summary>
    /// <param name="computerId">ID of computer</param>
    /// <param name="command">Action which should be performed (shutdown, restart, logoff)</param>
    /// <returns></returns>
    [HttpPatch("{computerId:int}/{command}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> SendCommand(int computerId, string command)
    {
        if (!WebSocketController.ComputerWebSockets.TryGetValue(computerId, out var computerWs))
            return NotFound();

        // check if websocket is still alive
        if (computerWs.State != WebSocketState.Open)
            return NotFound();

        // send command
        await computerWs.SendAsync(Encoding.UTF8.GetBytes(command).ToArray(), WebSocketMessageType.Text, true,
            CancellationToken.None);

        var computer = await context.Computers.FindAsync(computerId);
        context.Logs.Add(new Log
        {
            Message = $"Send {command} to '{computer?.Name}' (Id: {computerId})",
            Username = HttpContext.User.FindFirst("name")?.Value ?? "No username found in Token",
            Date = clock.GetCurrentInstant()
        });
        await context.SaveChangesAsync();
        return Ok();
    }
}