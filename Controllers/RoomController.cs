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
    public async Task<IActionResult> GetRoomTask(string? roomName, int roomId)
    {
        if (roomName != null)
            return await GetRoomByName(roomName);

        if (roomId != 0)
            return await GetComputersById(roomId);

        return BadRequest();
    }

    private async Task<IActionResult> GetRoomByName(string roomName)
    {
        var room = await _context.TabRooms.FirstOrDefaultAsync(x => x.Name.Contains(roomName));
        return Ok(room);
    }

    private async Task<IActionResult> GetComputersById(int roomId)
    {
        var computers = await _context.TabComputers.Where(x => x.Room == roomId).ToListAsync();
        return Ok(computers);
    }
}