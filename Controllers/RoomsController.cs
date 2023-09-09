using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    ///     <see cref="ApiModels.Room" />
    /// </returns>
    [HttpGet("{roomName}")]
    public async Task<IActionResult> GetRoomByName(string roomName)
    {
        var room = await _context.TabRooms.FirstOrDefaultAsync(x => x.Name != null && x.Name.Contains(roomName));
        return Ok(_mapper.Map<ApiModels.Room>(room));
    }

    /// <summary>
    ///     Find all Computers in a Room
    /// </summary>
    /// <param name="roomId">Id of room</param>
    /// <returns>
    ///     <see cref="ApiModels.Computer" />
    /// </returns>
    [HttpGet("{roomId:int}/computers")]
    public async Task<IActionResult> GetComputersInRoom(int roomId)
    {
        var computers = await _context.TabComputers.Where(x => x.RoomId == roomId).ToListAsync();
        return Ok(_mapper.Map<List<ApiModels.Computer>>(computers));
    }

    /// <summary>
    ///     Find all Lessons in a Room
    /// </summary>
    /// <param name="roomId">Id of room</param>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see cref="ApiModels.Lesson" /></returns>
    [HttpGet("{roomId:int}/lessons")]
    public async Task<IActionResult> GetLessonsInRoom(int roomId)
    {
        var todayLessons = await _context.TabLessons
            .Where(x => x.RoomId == roomId && x.StartTime.HasValue && x.StartTime.Value.Date == DateTime.Today)
            .ToListAsync();
        return Ok(_mapper.Map<List<ApiModels.Lesson>>(todayLessons));
    }

    /// <summary>
    ///     Find all available rooms
    /// </summary>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see cref="ApiModels.Room" /></returns>
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _context.TabRooms.Include(tabRoom => tabRoom.TabComputers)
            .Where(tabRoom => tabRoom.TabComputers.Count > 0).Select(room =>
                new ApiModels.Room(room.RoomId, room.Name!, room.LongName!, room.TabComputers.Count)).ToListAsync();
        return Ok(rooms);
    }
}