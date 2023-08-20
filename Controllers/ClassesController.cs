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
    /// Find class by Name
    /// </summary>
    /// <param name="name">Name of class</param>
    /// <returns><see cref="ApiModels.Class"/></returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetClass(string name)
    {
        if (await _context.TabClasses.FirstOrDefaultAsync(x => x.Name == name) is { } klasse)
            return Ok(_mapper.Map<ApiModels.Class>(klasse));
        return NotFound();
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
                if (klasse.AzureGroupID is not null)
                {
                    if (await _context.TabGroups.FindAsync(klasse.AzureGroupID) is null)
                        return NotFound($"{klasse.AzureGroupID} does not exist!");
                    dbClass.AzureGroupID = klasse.AzureGroupID;
                }
                _context.TabClasses.Update(dbClass);
            }
            else
            {
                if (klasse.AzureGroupID is null)
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