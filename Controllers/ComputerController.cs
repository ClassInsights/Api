using System.Net.NetworkInformation;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComputerController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    public ComputerController(ClassInsightsContext context) => _context = context;

    /// <summary>
    /// Is used to add or update an existing computer
    /// </summary>
    /// <param name="computer">Heartbeat object with the newest data from a computer</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> AddComputerTask(DbModels.TabComputers computer)
    {
        if (await _context.TabComputers.FindAsync(computer.Name) is { } pc)
        {
            pc.LastSeen = computer.LastSeen;
            pc.Room = computer.Room;
            pc.Mac = computer.Mac;
            pc.Ip = computer.Ip;
        }
        else await _context.TabComputers.AddAsync(computer);

        await _context.SaveChangesAsync();
        return Ok(computer);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> ShutdownComputer(int id)
    {
        var claims = HttpContext.User.Claims;
        var a = HttpContext.User.Identity.Name;
        return Ok();
    }
}