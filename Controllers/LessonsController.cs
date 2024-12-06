using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class LessonsController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    private readonly IMapper _mapper;

    /// <inheritdoc />
    public LessonsController(ClassInsightsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    ///     Find all available Lessons
    /// </summary>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see crefApiDto.LessonDtoon" /></returns>
    [HttpGet]
    public async Task<IActionResult> GetAllLessons()
    {
        var lessons = await _context.Lessons.ToListAsync();
        return Ok(_mapper.Map<List<ApiDto.LessonDto>>(lessons));
    }
}