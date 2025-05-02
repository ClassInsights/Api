using System.ComponentModel;
using Api.Models.Database;
using Api.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubjectsController(ClassInsightsContext context) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Find all subjects")]
    [ProducesResponseType<List<SubjectDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSubjects()
    {
        var subjects = await context.Subjects.AsNoTracking().ToListAsync();
        return Ok(subjects.Select(x => x.ToDto()).ToList());
    }

    [HttpGet("{subjectId:int}")]
    [EndpointSummary("Find subject by id")]
    [ProducesResponseType<SubjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubjectById([Description("Id of the subject you want to find")] int subjectId)
    {
        if (await context.Subjects.FindAsync(subjectId) is not { } subject)
            return NotFound();
        return Ok(subject.ToDto());
    }
}