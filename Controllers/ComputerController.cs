using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComputerController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    public ComputerController(ClassInsightsContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> AddComputerTask(DbModels.TabComputers computer)
    {
        var result = computer;
        if (await _context.TabComputers.FindAsync(computer.Name) is { } pc)
        {
            pc.LastSeen = computer.LastSeen;
            pc.Room = computer.Room;
            result = pc;
        }
        else await _context.TabComputers.AddAsync(computer);

        await _context.SaveChangesAsync();
        return Ok(result);
    }
}