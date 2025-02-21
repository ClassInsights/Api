using System.Text.RegularExpressions;
using Api.Models.Database;
using Api.Models.Dto;
using Api.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class RoomsController(ClassInsightsContext context, UntisService untisService, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Find a room by name
    /// </summary>
    /// <param name="roomName">Name of room</param>
    /// <returns>
    ///     <see cref="ApiDto.RoomDto" />
    /// </returns>
    [HttpGet("{roomName}")]
    public async Task<IActionResult> GetRoomByName(string roomName)
    {
        var room = await context.Rooms.FirstOrDefaultAsync(x => x.Regex != null && Regex.IsMatch(roomName, x.Regex));
        return Ok(mapper.Map<ApiDto.RoomDto>(room));
    }

    /// <summary>
    ///     Find all computers in a room
    /// </summary>
    /// <param name="roomId">ID of room</param>
    /// <returns>
    ///     <see cref="ApiDto.ComputerDto" />
    /// </returns>
    [HttpGet("{roomId:int}/computers")]
    public async Task<IActionResult> GetComputersInRoom(int roomId)
    {
        var computers = await context.Computers.AsNoTracking().Where(x => x.RoomId == roomId).ToListAsync();
        return Ok(mapper.Map<List<ApiDto.ComputerDto>>(computers));
    }

    /// <summary>
    ///     Find all lessons in a room
    /// </summary>
    /// <param name="roomId">ID of room</param>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see cref="ApiDto.LessonDto" /></returns>
    [HttpGet("{roomId:int}/lessons")]
    public async Task<IActionResult> GetLessonsInRoom(int roomId)
    {
        var tz = DateTimeZoneProviders.Bcl.GetSystemDefault();
        var today = SystemClock.Instance.GetCurrentInstant().InZone(tz).Date;

        var todayLessons = await context.Lessons.AsNoTracking()
            .Where(x => x.RoomId == roomId && x.Start.HasValue && x.Start.Value.InZone(tz).Date == today)
            .ToListAsync();
        return Ok(mapper.Map<List<ApiDto.LessonDto>>(todayLessons));
    }

    /// <summary>
    ///     Find all available rooms
    /// </summary>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see cref="ApiDto.RoomDto" /></returns>
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await context.Rooms.AsNoTracking().Include(dbRoom => dbRoom.Computers)
            .Where(dbRoom => dbRoom.Computers.Count > 0).Select(room =>
                new ApiDto.RoomDto(room.RoomId, room.DisplayName!, room.Computers.Count)).ToListAsync();
        return Ok(rooms);
    }
    
    /// <summary>
    ///     Force refresh for all rooms and untis records
    /// </summary>
    /// <returns></returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiDto.LessonDto>> RefreshAll()
    {
        await untisService.UpdateUntisRecords(true);
        return Ok();
    }
}