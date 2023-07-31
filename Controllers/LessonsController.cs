using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        if (roomId != 0)
            return await GetLessonsById(roomId);
        return await GetAllLessons();
    }

    private async Task<IActionResult> GetAllLessons()
    {
        var lessons = await _context.TabLessons.ToListAsync();
        return Ok(lessons);
    }

    private async Task<IActionResult> GetLessonsById(int roomId)
    {
        var lessons = await _context.TabLessons.Where(x => x.Room == roomId).ToListAsync();
        return Ok(lessons.Where(x => x.StartTime.DayOfWeek == DateTime.Now.DayOfWeek).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> AddLessons(List<DbModels.TabLessons> lessons)
    {
        if (!lessons.Any()) return Ok();

        // delete all lessons
        await _context.TabLessons.ExecuteDeleteAsync();

        // reset auto increment id
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT (tabLessons, RESEED, 0);");

        // add new lessons
        await _context.TabLessons.AddRangeAsync(lessons);
        await _context.SaveChangesAsync();

        return Ok();
    }
}