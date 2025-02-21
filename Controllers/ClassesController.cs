using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using SystemTextJsonPatch;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class ClassesController(IClock clock, ClassInsightsContext context, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Find all classes
    /// </summary>
    /// <returns>
    ///     <see cref="List{T}" /> whose generic type argument is <see cref="ApiDto.ClassDto" />
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAllClasses()
    {
        var classes = await context.Classes.AsNoTracking().ToListAsync();
        return Ok(mapper.Map<List<ApiDto.ClassDto>>(classes));
    }


    /// <summary>
    ///     Modify a class
    /// </summary>
    /// <param name="classId">ID of class</param>
    /// <param name="patchDocument">New values for Class</param>
    /// <returns></returns>
    [HttpPatch("{classId:int}")]
    public async Task<IActionResult> UpdateClass(int classId, JsonPatchDocument<ApiDto.ClassDto>? patchDocument)
    {
        if (patchDocument == null || classId < 1)
            return BadRequest();
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var dbClass = await context.Classes.FindAsync(classId);
        if (dbClass == null)
            return NotFound();
        
        var modelClass = mapper.Map<ApiDto.ClassDto>(dbClass);
        patchDocument.ApplyTo(modelClass);

        context.Update(mapper.Map<Class>(modelClass));
        await context.SaveChangesAsync();

        return Ok(modelClass);
    }

    /// <summary>
    ///     Find class by name
    /// </summary>
    /// <param name="name">Name of class</param>
    /// <returns>
    ///     <see cref="ApiDto.ClassDto" />
    /// </returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetClass(string name)
    {
        if (await context.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.DisplayName == name) is { } dbClass)
            return Ok(mapper.Map<ApiDto.ClassDto>(dbClass));
        return NotFound();
    }

    /// <summary>
    ///     Find Class by ID
    /// </summary>
    /// <param name="classId">ID of specific class</param>
    /// <returns>
    ///     <see cref="ApiDto.ClassDto" />
    /// </returns>
    [HttpGet("{classId:int}")]
    public async Task<IActionResult> GetClassById(int classId)
    {
        if (await context.Classes.FindAsync(classId) is not { } dbClass)
            return NotFound();
        return Ok(mapper.Map<ApiDto.ClassDto>(dbClass));
    }

    /// <summary>
    ///     Find current Lesson of Class by ID
    /// </summary>
    /// <param name="classId">ID of specific class</param>
    /// <returns>
    ///     <see cref="ApiDto.LessonDto" />
    /// </returns>
    [HttpGet("{classId:int}/currentLesson")]
    public async Task<IActionResult> GetCurrentLesson(int classId)
    {
        // receive all lessons of class
        var lessons = await context.Lessons.Where(x => x.ClassId == classId).ToListAsync();

        // check minimum positive of difference between now and future
        var currentLesson = lessons
            .Where(x => (x.End - clock.GetCurrentInstant())?.TotalMilliseconds > 0)
            .MinBy(x => x.End - clock.GetCurrentInstant());

        // all lessons are over
        if (currentLesson == null)
            return Ok();

        return Ok(mapper.Map<ApiDto.LessonDto>(currentLesson));
    }
}