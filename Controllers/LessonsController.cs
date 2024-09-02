using Api.Attributes;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    /// <returns><see cref="List{T}" /> whose generic type argument is <see cref="ApiModels.Lesson" /></returns>
    [HttpGet]
    public async Task<IActionResult> GetAllLessons()
    {
        var lessons = await _context.Lessons.ToListAsync();
        return Ok(_mapper.Map<List<ApiModels.Lesson>>(lessons));
    }

    /// <summary>
    ///     Replaces all Lessons with the new
    /// </summary>
    /// <param name="lessons">List of new lessons</param>
    /// <returns></returns>
    [HttpPost]
    [IsLocal]
    [AllowAnonymous]
    public async Task<IActionResult> AddLessons(List<ApiModels.Lesson> lessons)
    {
        if (!lessons.Any()) return Ok();

        // delete all lessons
        await _context.Lessons.ExecuteDeleteAsync();
        
        // reset auto increment id
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT (tabLesson, RESEED, 0);");

        // add new lessons
        await _context.Lessons.AddRangeAsync(_mapper.Map<List<Lesson>>(lessons));
        await _context.SaveChangesAsync();

        return Ok();
    }
}