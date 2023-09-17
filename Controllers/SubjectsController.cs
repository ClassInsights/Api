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
public class SubjectsController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    private readonly IMapper _mapper;

    /// <inheritdoc />
    public SubjectsController(ClassInsightsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    ///     Find all Subjects
    /// </summary>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see cref="ApiModels.Subject" /></returns>
    [HttpGet]
    public async Task<IActionResult> GetAllSubjects()
    {
        return Ok(_mapper.Map<List<ApiModels.Subject>>(await _context.TabSubjects.ToListAsync()));
    }

    /// <summary>
    ///     Find specific Subject by Id
    /// </summary>
    /// <param name="subjectId">Id of Subject</param>
    /// <returns>
    ///     <see cref="ApiModels.Subject" />
    /// </returns>
    [HttpGet("{subjectId:int}")]
    public async Task<IActionResult> GetSubjectById(int subjectId)
    {
        if (await _context.TabSubjects.FindAsync(subjectId) is not { } subject)
            return NotFound();
        return Ok(_mapper.Map<ApiModels.Subject>(subject));
    }

    /// <summary>
    ///     Adds or deletes subjects
    /// </summary>
    /// <param name="subjects">List of all Subjects</param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [IsLocal]
    public async Task<IActionResult> AddOrDeleteSubjects(List<ApiModels.Subject> subjects)
    {
        var dbSubjects = await _context.TabSubjects.ToListAsync();
        var newSubjects = subjects
            .Where(subject => dbSubjects.All(dbSubject => subject.SubjectId != dbSubject.SubjectId)).ToList();

        var oldSubjects = dbSubjects
            .Where(dbSubject => subjects.All(subject => dbSubject.SubjectId != subject.SubjectId)).ToList();

        _context.TabSubjects.AddRange(_mapper.Map<List<TabSubject>>(newSubjects));
        _context.TabSubjects.RemoveRange(oldSubjects);

        await _context.SaveChangesAsync();
        return Ok();
    }
}