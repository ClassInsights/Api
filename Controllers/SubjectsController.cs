using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubjectsController(ClassInsightsContext context, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Find all subjects
    /// </summary>
    /// <returns><see cref="List{T}" /> whose generic type argument is <see cref="ApiDto.SubjectDto" /></returns>
    [HttpGet]
    public async Task<IActionResult> GetAllSubjects()
    {
        return Ok(mapper.Map<List<ApiDto.SubjectDto>>(await context.Subjects.ToListAsync()));
    }

    /// <summary>
    ///     Find specific subject by ID
    /// </summary>
    /// <param name="subjectId">ID of subject</param>
    /// <returns>
    ///     <see cref="ApiDto.SubjectDto" />
    /// </returns>
    [HttpGet("{subjectId:int}")]
    public async Task<IActionResult> GetSubjectById(int subjectId)
    {
        if (await context.Subjects.FindAsync(subjectId) is not { } subject)
            return NotFound();
        return Ok(mapper.Map<ApiDto.SubjectDto>(subject));
    }
}