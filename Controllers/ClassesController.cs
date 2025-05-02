using System.ComponentModel;
using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClassesController(IClock clock, ClassInsightsContext context, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Find all classes")]
    [ProducesResponseType<List<ClassDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllClasses()
    {
        var classes = await context.Classes.AsNoTracking().ToListAsync();
        return Ok(mapper.Map<List<ClassDto>>(classes));
    }

    [HttpGet("{name}")]
    [EndpointSummary("Find class by name")]
    [ProducesResponseType<ClassDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClass([Description("The name of the class you search")] string name)
    {
        if (await context.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.DisplayName == name) is { } dbClass)
            return Ok(mapper.Map<ClassDto>(dbClass));
        return NotFound();
    }

    [HttpGet("{classId:int}")]
    [EndpointSummary("Find Class by id")]
    [ProducesResponseType<ClassDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassById([Description("The id of the class you search")] int classId)
    {
        if (await context.Classes.FindAsync(classId) is not { } dbClass)
            return NotFound();
        return Ok(mapper.Map<ClassDto>(dbClass));
    }

    [HttpGet("{classId:int}/currentLesson")]
    [EndpointSummary("Find the current lesson of a class by id")]
    [ProducesResponseType<LessonDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentLesson(
        [Description("The id of the class of which you want the current lesson")] int classId)
    {
        // receive all lessons of class
        var lessons = await context.Lessons.Where(x => x.ClassId == classId).ToListAsync();

        // check the minimum positive of difference between now and future
        var currentLesson = lessons
            .Where(x => (x.End - clock.GetCurrentInstant())?.TotalMilliseconds > 0)
            .MinBy(x => x.End - clock.GetCurrentInstant());

        // all lessons are over
        if (currentLesson == null)
            return Ok();

        return Ok(mapper.Map<LessonDto>(currentLesson));
    }
}