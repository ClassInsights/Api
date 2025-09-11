using System.ComponentModel;
using System.Text.RegularExpressions;
using Api.Models.Database;
using Api.Models.Dto;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Extensions;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomsController(IClock clock, ClassInsightsContext context, UntisService untisService)
    : ControllerBase
{
    [HttpPatch("{roomId:long}")]
    [AllowAnonymous]
    [EndpointSummary("Update a room by id")]
    public async Task<IActionResult> UpdateRoom([Description("Id of the room you want to update")] long roomId,
        [Description("The new values for the room")]
        RoomDto roomDto)
    {
        var room = await context.Rooms.FindAsync(roomId);
        if (room is null)
            return NotFound();

        room.OrganizationUnit = roomDto.OrganizationUnit;
        room.Enabled = roomDto.Enabled;

        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("{roomId:int}/computers")]
    [EndpointSummary("Find all computers in a room")]
    [ProducesResponseType<List<ComputerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetComputersInRoom(
        [Description("Id of the room you want to find the computers")]
        int roomId)
    {
        var computers = await context.Computers.AsNoTracking().Where(x => x.RoomId == roomId).ToListAsync();
        return Ok(computers.Select(x => x.ToDto()).ToList());
    }

    [HttpGet("{roomId:int}/lessons")]
    [EndpointSummary("Find all lessons in a room")]
    [ProducesResponseType<List<LessonDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetLessonsInRoom(
        [Description("Id of the room you want to find the lessons")]
        int roomId)
    {
        var tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
        var today = clock.InTzdbSystemDefaultZone().GetCurrentDate();

        var todayLessons = await context.Lessons.AsNoTracking()
            .Where(x => x.RoomId == roomId && x.Start.HasValue && x.Start.Value.InZone(tz).Date == today)
            .ToListAsync();
        return Ok(todayLessons.Select(x => x.ToDto()).ToList());
    }

    [HttpGet]
    [EndpointSummary("Find all available rooms")]
    [ProducesResponseType<List<RoomDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await context.Rooms.AsNoTracking().Include(dbRoom => dbRoom.Computers).Select(room =>
            new RoomDto(room.RoomId, room.Enabled, room.DisplayName!, room.OrganizationUnit, room.Computers.Count)).ToListAsync();
        return Ok(rooms);
    }

    [HttpPost("refresh")]
    [EndpointSummary("Force refresh all untis records")]
    public async Task<ActionResult<LessonDto>> RefreshAll()
    {
        await untisService.UpdateUntisRecords(true);
        return Ok();
    }
}