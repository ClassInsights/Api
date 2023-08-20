using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClassesController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    private readonly IMapper _mapper;

    public ClassesController(ClassInsightsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Get information from Class
    /// </summary>
    /// <param name="classId">Id of specific class</param>
    /// <param name="search">Type of which information should be returned</param>
    /// <returns>Information which was requested with <see cref="search"/></returns>
    [HttpGet("{classId:int}")]
    public async Task<IActionResult> GetClassInformation(int classId, string search = "currentLesson")
    {
        if (search != "currentLesson")
            return BadRequest();

        // receive all lessons of class
        var lessons = await _context.TabLessons.Where(x => x.ClassId == classId).ToListAsync();
        
        // check minimum positive of difference between now and future
        var currentLesson = lessons.Where(x => (x.EndTime - DateTime.Now)?.TotalMilliseconds > 0).MinBy(x => x.EndTime - DateTime.Now);

        // all lessons are over
        if (currentLesson == null)
            return Ok();

        return Ok(_mapper.Map<ApiModels.Lesson>(currentLesson));
    }

    [HttpPost]
    public async Task<IActionResult> AddOrUpdateClasses(List<ApiModels.Class> classes)
    {
        if (!classes.Any()) return Ok();

        // add or update classes
        foreach (var klasse in classes)
        {
            if (await _context.TabClasses.FindAsync(klasse.ClassId) is { } dbClass)
            {
                dbClass.Name = klasse.Name;
                dbClass.Head = klasse.Head;
                if (klasse.Group is not null)
                {
                    if (await _context.TabGroups.FindAsync(klasse.Group) is null)
                        return NotFound($"{klasse.Group} does not exist!");
                    dbClass.AzureGroupID = klasse.Group;
                }
                _context.TabClasses.Update(dbClass);
            }
            else
            {
                if (klasse.Group is null)
                {
                    // todo: microsoft api get group id
                }
                _context.TabClasses.Add(_mapper.Map<TabClass>(klasse));
            }
        }

        // delete old classes
        var dbClasses = await _context.TabClasses.ToListAsync();
        var oldClasses = dbClasses.Where(dbClass => classes.All(c => c.ClassId != dbClass.ClassId)).ToList();
        if(oldClasses.Any())
            _context.RemoveRange(oldClasses);
        
        await _context.SaveChangesAsync();
        return Ok();
    }
}