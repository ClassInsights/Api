using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LessonsController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    public LessonsController(ClassInsightsContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetLessons(int roomId)
    {
        var lessons = await _context.TabLessons.Where(x => x.Room == roomId).ToListAsync();
        return Ok(lessons.Where(x => x.StartTime.DayOfWeek == DateTime.Now.DayOfWeek).ToList());
    }
}