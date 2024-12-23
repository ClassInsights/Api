using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class LessonsController(ClassInsightsContext context, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Find all available Lessons
    /// </summary>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see cref="ApiDto.LessonDto" /></returns>
    [HttpGet]
    public async Task<IActionResult> GetAllLessons()
    {
        var lessons = await context.Lessons.AsNoTracking().ToListAsync();
        return Ok(mapper.Map<List<ApiDto.LessonDto>>(lessons));
    }
}