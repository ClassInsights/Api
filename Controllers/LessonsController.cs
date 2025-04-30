using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LessonsController(ClassInsightsContext context, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Find all available Lessons")]
    [ProducesResponseType<LessonDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLessons()
    {
        var lessons = await context.Lessons.AsNoTracking().ToListAsync();
        return Ok(mapper.Map<List<LessonDto>>(lessons));
    }
}