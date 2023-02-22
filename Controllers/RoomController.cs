using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    public RoomController(ClassInsightsContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetRoomTask(string roomName)
    {
        var room = await _context.TabRooms.FirstOrDefaultAsync(x => x.Name.Contains(roomName));
        return Ok(room);
    }
}