using Api.Attributes;
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
        // room names in db must start with name DV and number e.g. DV206
        var room = await _context.Rooms.FirstOrDefaultAsync(x => x.DisplayName != null && x.DisplayName.Contains(roomName.Substring(0,3)));
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
    [AllowAnonymous]
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

    /// <summary>
    ///     Adds or deletes Rooms
    /// </summary>
    /// <param name="rooms">List of Rooms</param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [IsLocal]
    public async Task<IActionResult> AddOrDeleteRooms(List<ApiDto.RoomDto> rooms)
    {
        var dbRooms = await _context.Rooms.ToListAsync();
        var newRooms = rooms
            .Where(room => dbRooms.All(dbRoom => room.RoomId != dbRoom.RoomId)).ToList();

        var oldRooms = dbRooms
            .Where(dbRoom => rooms.All(room => dbRoom.RoomId != room.RoomId)).ToList();

        _context.Rooms.AddRange(_mapper.Map<List<Room>>(newRooms));
        _context.Rooms.RemoveRange(oldRooms);

        await _context.SaveChangesAsync();
        return Ok();
    }
}