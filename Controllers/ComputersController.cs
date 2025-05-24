using System.ComponentModel;
using System.Net.WebSockets;
using System.Text;
using Api.Models.Database;
using Api.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComputersController(ClassInsightsContext context) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Computer")]
    [EndpointSummary("Add or update a computer")]
    [ProducesResponseType<ComputerDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateComputer(
        [Description("Computer which you want to add or update")]
        ComputerDto computerDto)
    {
        // map to the database object to receive new ComputerId if it was created
        var dbComputer = computerDto.ToComputer();
        context.Update(dbComputer);
        await context.SaveChangesAsync();

        return Ok(dbComputer.ToDto());
    }

    [HttpGet]
    [EndpointSummary("Find all computers")]
    [ProducesResponseType<List<ComputerDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComputers()
    {
        var computers = await context.Computers.AsNoTracking().ToListAsync();
        return Ok(computers.Select(x => x.ToDto()));
    }

    [HttpGet("{name}")]
    [EndpointSummary("Get information of computer by name")]
    [ProducesResponseType<ComputerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComputer(string name)
    {
        if (await context.Computers.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Equals(name)) is { } computer)
            return Ok(computer.ToDto());
        return NotFound();
    }

    [HttpPost("commands")]
    [EndpointSummary("Send commands to computers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = "Teacher, Admin")]
    public IActionResult SendCommands(List<SendCommandDto> entries)
    {
        foreach (var entry in entries)
            _ = Task.Run(async () => await SendCommand(entry.ComputerId, entry.Command));

        // todo: re-add logging
        return Ok();
    }

    private static async Task SendCommand(long computerId, string command)
    {
        if (!WebSocketController.ComputerWebSockets.TryGetValue(computerId, out var computerWs))
            return;

        // check if websocket is still alive
        if (computerWs.State != WebSocketState.Open)
            return;

        // send command
        await computerWs.SendAsync(Encoding.UTF8.GetBytes(command), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}