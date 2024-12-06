using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class ClassesController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    private readonly IMapper _mapper;

    /// <inheritdoc />
    public ClassesController(ClassInsightsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    /// <summary>
    ///     Find all classes
    /// </summary>
    /// <returns>
    ///     <see cref="List{T}" /> whose generic type argument is <see crefApiDto.ClassDtoss" />
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAllClasses()
    {
        var classes = await _context.Classes.AsNoTracking().ToListAsync();
        return Ok(_mapper.Map<List<ApiDto.ClassDto>>(classes));
    }

    
    /// <summary>
    /// Modify classes
    /// </summary>
    /// <param name="patchDocument">New values for Class</param>
    /// <returns></returns>
    [HttpPatch]
    public async Task<IActionResult> UpdateClass(JsonPatchDocument<List<ApiDto.ClassDto>>? patchDocument)
    {
        var classes = await _context.Classes.AsNoTracking().ToListAsync();
        
        if (patchDocument == null)
            return BadRequest();

        var modelClasses = _mapper.Map<List<ApiDto.ClassDto>>(classes);
        patchDocument.ApplyTo(modelClasses, ModelState);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        _context.UpdateRange(_mapper.Map<List<Class>>(modelClasses));
        await _context.SaveChangesAsync();
        
        return Ok(modelClasses);
    }

    /// <summary>
    ///     Find class by Name
    /// </summary>
    /// <param name="name">Name of class</param>
    /// <returns>
    ///     <see crefApiDto.ClassDtoss" />
    /// </returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetClass(string name)
    {
        if (await _context.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.DisplayName == name) is { } klasse)
            return Ok(_mapper.Map<ApiDto.ClassDto>(klasse));
        return NotFound();
    }

    /// <summary>
    ///     Find Class by Id
    /// </summary>
    /// <param name="classId">Id of specific class</param>
    /// <returns>
    ///     <see crefApiDto.ClassDtoss" />
    /// </returns>
    [HttpGet("{classId:int}")]
    public async Task<IActionResult> GetClassById(int classId)
    {
        if (await _context.Classes.FindAsync(classId) is not { } klasse)
            return NotFound();
        return Ok(_mapper.Map<ApiDto.ClassDto>(klasse));
    }

    /// <summary>
    ///     Find current Lesson of Class by Id
    /// </summary>
    /// <param name="classId">Id of specific class</param>
    /// <returns>
    ///     <see crefApiDto.LessonDtoon" />
    /// </returns>
    [HttpGet("{classId:int}/currentLesson")]
    public async Task<IActionResult> GetCurrentLesson(int classId)
    {
        // receive all lessons of class
        var lessons = await _context.Lessons.Where(x => x.ClassId == classId).ToListAsync();

        // check minimum positive of difference between now and future
        var currentLesson = lessons.Where(x => (x.End - SystemClock.Instance.GetCurrentInstant())?.TotalMilliseconds > 0)
            .MinBy(x => x.End - SystemClock.Instance.GetCurrentInstant());

        // all lessons are over
        if (currentLesson == null)
            return Ok();

        return Ok(_mapper.Map<ApiDto.LessonDto>(currentLesson));
    }
}