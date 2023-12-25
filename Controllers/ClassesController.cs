using Api.Attributes;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class ClassesController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ClassInsightsContext _context;
    private readonly GraphServiceClient _graphClient;
    private readonly IMapper _mapper;

    /// <inheritdoc />
    public ClassesController(ClassInsightsContext context, GraphServiceClient graphClient, IConfiguration config,
        IMapper mapper)
    {
        _context = context;
        _graphClient = graphClient;
        _config = config;
        _mapper = mapper;
    }
    
    /// <summary>
    ///     Find all classes
    /// </summary>
    /// <returns>
    ///     <see cref="List{T}" /> whose generic type argument is <see cref="ApiModels.Class" />
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAllClasses()
    {
        var classes = await _context.TabClasses.AsNoTracking().ToListAsync();
        return Ok(_mapper.Map<List<ApiModels.Class>>(classes));
    }

    
    /// <summary>
    /// Modify classes
    /// </summary>
    /// <param name="patchDocument">New values for Class</param>
    /// <returns></returns>
    [HttpPatch]
    public async Task<IActionResult> UpdateClass(JsonPatchDocument<List<ApiModels.Class>>? patchDocument)
    {
        var classes = await _context.TabClasses.AsNoTracking().ToListAsync();
        
        if (patchDocument == null)
            return BadRequest();

        var modelClasses = _mapper.Map<List<ApiModels.Class>>(classes);
        patchDocument.ApplyTo(modelClasses, ModelState);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        _context.UpdateRange(_mapper.Map<List<TabClass>>(modelClasses));
        await _context.SaveChangesAsync();
        
        return Ok(modelClasses);
    }

    /// <summary>
    ///     Find class by Name
    /// </summary>
    /// <param name="name">Name of class</param>
    /// <returns>
    ///     <see cref="ApiModels.Class" />
    /// </returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetClass(string name)
    {
        if (await _context.TabClasses.AsNoTracking().FirstOrDefaultAsync(x => x.Name == name) is { } klasse)
            return Ok(_mapper.Map<ApiModels.Class>(klasse));
        return NotFound();
    }

    /// <summary>
    ///     Find Class by Id
    /// </summary>
    /// <param name="classId">Id of specific class</param>
    /// <returns>
    ///     <see cref="ApiModels.Class" />
    /// </returns>
    [HttpGet("{classId:int}")]
    public async Task<IActionResult> GetClassById(int classId)
    {
        if (await _context.TabClasses.FindAsync(classId) is not { } klasse)
            return NotFound();
        return Ok(_mapper.Map<ApiModels.Class>(klasse));
    }

    /// <summary>
    ///     Find current Lesson of Class by Id
    /// </summary>
    /// <param name="classId">Id of specific class</param>
    /// <returns>
    ///     <see cref="ApiModels.Lesson" />
    /// </returns>
    [HttpGet("{classId:int}/currentLesson")]
    public async Task<IActionResult> GetCurrentLesson(int classId)
    {
        // receive all lessons of class
        var lessons = await _context.TabLessons.Where(x => x.ClassId == classId).ToListAsync();

        // check minimum positive of difference between now and future
        var currentLesson = lessons.Where(x => (x.EndTime - DateTime.Now)?.TotalMilliseconds > 0)
            .MinBy(x => x.EndTime - DateTime.Now);

        // all lessons are over
        if (currentLesson == null)
            return Ok();

        return Ok(_mapper.Map<ApiModels.Lesson>(currentLesson));
    }

    /// <summary>
    ///     Adds new Classes and deletes old
    /// </summary>
    /// <param name="classes">List of all Classes</param>
    /// <returns></returns>
    [HttpPost]
    [IsLocal]
    [AllowAnonymous]
    public async Task<IActionResult> AddOrDeleteClasses(List<ApiModels.Class> classes)
    {
        // retrieve all classes from db
        var dbClasses = await _context.TabClasses.ToListAsync();

        // add new classes
        foreach (var klasse in classes.Where(klasse => dbClasses.All(dbClass => dbClass.ClassId != klasse.ClassId)))
        {
            // build azure group name
            if (!int.TryParse(klasse.Name[..1], out var grade))
                continue;
            
            var startDate = DateTime.Parse(_config["Dashboard:SchoolYear:StartDate"]!);
            var startYear = startDate.Year - grade + 1;
            var type = klasse.Name[1..];

            var groupName = _config["Dashboard:AzureGroupPattern"]?.Replace("YEAR", startYear.ToString())
                .Replace("CLASS", type);
            var groups = await _graphClient.Groups.GetAsync(configuration =>
            {
                configuration.QueryParameters.Filter = $"displayName eq '{groupName}'";
                configuration.Options.WithAppOnly();
                configuration.Options.WithAuthenticationScheme("OpenIdConnect");
            });

            klasse.AzureGroupId = groups?.Value?.FirstOrDefault()?.Id;
            
            // set azureId
            _context.TabClasses.Add(_mapper.Map<TabClass>(klasse));
        }

        // delete old classes from db
        var oldClasses = dbClasses.Where(dbClass => classes.All(klasse => klasse.ClassId != dbClass.ClassId)).ToList();
        if (oldClasses.Count > 0)
            _context.RemoveRange(oldClasses);

        await _context.SaveChangesAsync();
        return Ok();
    }
}