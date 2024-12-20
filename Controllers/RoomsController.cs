﻿using System.Text.RegularExpressions;
using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    private readonly IMapper _mapper;

    /// <inheritdoc />
    public RoomsController(ClassInsightsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    ///     Find a Room by it's name
    /// </summary>
    /// <param name="roomName">Name of room</param>
    /// <returns>
    ///     <see crefApiDto.RoomDtoom" />
    /// </returns>
    [HttpGet("{roomName}")]
    public async Task<IActionResult> GetRoomByName(string roomName)
    {
        // computer name must contain room name 
        var room = await _context.Rooms.FirstOrDefaultAsync(x => x.DisplayName != null && (x.Regex != null ? Regex.IsMatch(roomName, x.Regex) : roomName.Contains(x.DisplayName)));
        return Ok(_mapper.Map<ApiDto.RoomDto>(room));
    }

    /// <summary>
    ///     Find all Computers in a Room
    /// </summary>
    /// <param name="roomId">Id of room</param>
    /// <returns>
    ///     <see crefApiDto.ComputerDtoer" />
    /// </returns>
    [HttpGet("{roomId:int}/computers")]
    public async Task<IActionResult> GetComputersInRoom(int roomId)
    {
        var computers = await _context.Computers.Where(x => x.RoomId == roomId).ToListAsync();
        return Ok(_mapper.Map<List<ApiDto.ComputerDto>>(computers));
    }

    /// <summary>
    ///     Find all Lessons in a Room
    /// </summary>
    /// <param name="roomId">Id of room</param>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see crefApiDto.LessonDtoon" /></returns>
    [HttpGet("{roomId:int}/lessons")]
    public async Task<IActionResult> GetLessonsInRoom(int roomId)
    {
        var tz = DateTimeZoneProviders.Bcl.GetSystemDefault();
        var today = SystemClock.Instance.GetCurrentInstant().InZone(tz).Date;
        
        var todayLessons = await _context.Lessons
            .Where(x => x.RoomId == roomId && x.Start.HasValue && x.Start.Value.InZone(tz).Date == today)
            .ToListAsync();
        return Ok(_mapper.Map<List<ApiDto.LessonDto>>(todayLessons));
    }

    /// <summary>
    ///     Find all available rooms
    /// </summary>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see crefApiDto.RoomDtoom" /></returns>
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _context.Rooms.Include(tabRoom => tabRoom.Computers)
            .Where(tabRoom => tabRoom.Computers.Count > 0).Select(room =>
                new ApiDto.RoomDto(room.RoomId, room.DisplayName!, room.Computers.Count)).ToListAsync();
        return Ok(rooms);
    }
}