using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    private readonly IMapper _mapper;

    public RoomController(ClassInsightsContext context, IMapper mapper)
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