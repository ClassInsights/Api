using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    private readonly IMapper _mapper;

    public RoomsController(ClassInsightsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("{roomName}")]
    public async Task<IActionResult> GetRoomByName(string roomName)
    {
        var room = await _context.TabRooms.FirstOrDefaultAsync(x => x.Name != null && x.Name.Contains(roomName));
        return Ok(_mapper.Map<ApiModels.Room>(room));
    }

    [HttpGet("{roomId:int}")]
    public async Task<IActionResult> GetComputersById(int roomId, string search = "computers")
    {
        return search switch
        {
            "computers" => await GetComputers(roomId),
            "lessons" => await GetLessons(roomId),
            _ => BadRequest()
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _context.TabRooms.Include(tabRoom => tabRoom.TabComputers).ToListAsync();
        var responseRooms = rooms.Select(room => new ApiModels.Room(room.RoomId, room.Name!, room.LongName!, room.TabComputers.Count)).ToList();
        return Ok(responseRooms);
    }

    private async Task<IActionResult> GetComputers(int roomId)
    {
        var computers = await _context.TabComputers.Where(x => x.RoomId == roomId).ToListAsync();
        return Ok(_mapper.Map<List<ApiModels.Computer>>(computers));
    }

    private async Task<IActionResult> GetLessons(int roomId)
    {
        var lessons = await _context.TabLessons.Where(x => x.RoomId == roomId).ToListAsync();
        var todayLessons = lessons.Where(x => x.StartTime?.DayOfWeek == DateTime.Now.DayOfWeek).ToList();
        return Ok(_mapper.Map<List<ApiModels.Lesson>>(todayLessons));
    }
}