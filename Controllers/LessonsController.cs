using Api.Models.Database;
using Api.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LessonsController(ClassInsightsContext context) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Find all available Lessons")]
    [ProducesResponseType<List<LessonDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLessons()
    {
        var lessons = await context.Lessons.AsNoTracking().ToListAsync();
        return Ok(lessons.Select(x => x.ToDto()).ToList());
    }
}