using System.ComponentModel;
using System.Net.WebSockets;
using System.Text;
using Api.Models.Database;
using Api.Models.Dto;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComputersController(ClassInsightsContext context, SettingsService settingsService) : ControllerBase
{
    [HttpPatch]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Update multiple computers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateComputers(
        [FromBody] [Description("List of computer which you want to update")] List<ComputerDto> computers)
    {
        if (computers.Count == 0)
            return BadRequest("No Computers provided.");

        var ids = computers.Select(c => c.ComputerId).ToList();
        var dbComputers = await context.Computers.Where(c => ids.Contains(c.ComputerId)).ToListAsync();

        foreach (var dto in computers)
        {
            var dbComputer = dbComputers.FirstOrDefault(c => c.ComputerId == dto.ComputerId);
            if (dbComputer == null)
                continue;

            dbComputer.RoomId = dto.RoomId;
            dbComputer.Name = dto.Name;
            dbComputer.MacAddress = dto.MacAddress;
            dbComputer.IpAddress = dto.IpAddress;
            dbComputer.LastSeen = dto.LastSeen;
            dbComputer.LastUser = dto.LastUser;
            dbComputer.Version = dto.Version;
        }

        await context.SaveChangesAsync();
        return Ok();
    }


    [HttpPost]
    [Authorize(Roles = "Computer")]
    [EndpointSummary("Add or update a computer")]
    [ProducesResponseType<ComputerDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateComputer(
        [Description("Computer which you want to add or update")]
        ComputerDto computerDto)
    {
        var credentials = await settingsService.GetSettingAsync<SettingsDto.AdCredentials>("ad");
        
        // map to the database object to receive new ComputerId if it was created
        var dbComputer = computerDto.ToComputer();
        
        // ad sync
        if (credentials != null && (dbComputer.RoomId == null || credentials.AutoSync))
        {
            var room = await context.Rooms.FirstOrDefaultAsync(x => x.OrganizationUnit == computerDto.OrganizationUnit);
            if (room != null)
            {
                dbComputer.RoomId = room.RoomId;
            }
        }
        
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