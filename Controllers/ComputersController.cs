using System.ComponentModel;
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

[Route("api/[controller]")]
[ApiController]
public class ComputersController(IClock clock, ClassInsightsContext context, IMapper mapper) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Computer")]
    [EndpointSummary("Add or update a computer")]
    public async Task<IActionResult> UpdateComputer(
        [Description("Computer which you want to add or update")] ComputerDto computerDto)
    {
        // map to the database object to receive new ComputerId if it was created
        var dbComputer = mapper.Map<Computer>(computerDto);
        context.Update(dbComputer);
        await context.SaveChangesAsync();

        return Ok(mapper.Map<ComputerDto>(dbComputer));
    }

    [HttpGet]
    [EndpointSummary("Find all computers")]
    public async Task<IActionResult> GetComputers()
    {
        var computers = await context.Computers.AsNoTracking().ToListAsync();
        return Ok(mapper.Map<List<ComputerDto>>(computers));
    }

    [HttpGet("{name}")]
    [EndpointSummary("Get information of computer by name")]
    [ProducesResponseType<ComputerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComputer(string name)
    {
        if (await context.Computers.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains(name)) is { } computer)
            return Ok(mapper.Map<ComputerDto>(computer));
        return NotFound();
    }

    [HttpPatch("{computerId:int}/{command}")]
    [Authorize(Roles = "Teacher, Admin")]
    [EndpointSummary("Send command to computer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendCommand([Description("ID of the target computer")] int computerId,
        [Description("Action which should be performed (shutdown, restart, logoff)")] string command)
    {
        if (!WebSocketController.ComputerWebSockets.TryGetValue(computerId, out var computerWs))
            return NotFound();

        // check if websocket is still alive
        if (computerWs.State != WebSocketState.Open)
            return NotFound();

        // send command
        await computerWs.SendAsync(Encoding.UTF8.GetBytes(command), WebSocketMessageType.Text, true,
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