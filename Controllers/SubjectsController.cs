using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
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
    /// <returns><see cref="List{T}" /> whose generic type argument is <see crefApiDto.SubjectDtoct" /></returns>
    [HttpGet]
    public async Task<IActionResult> GetAllSubjects()
    {
        return Ok(_mapper.Map<List<ApiDto.SubjectDto>>(await _context.Subjects.ToListAsync()));
    }

    /// <summary>
    ///     Find specific Subject by Id
    /// </summary>
    /// <param name="subjectId">Id of Subject</param>
    /// <returns>
    ///     <see crefApiDto.SubjectDtoct" />
    /// </returns>
    [HttpGet("{subjectId:int}")]
    public async Task<IActionResult> GetSubjectById(int subjectId)
    {
        if (await _context.Subjects.FindAsync(subjectId) is not { } subject)
            return NotFound();
        return Ok(_mapper.Map<ApiDto.SubjectDto>(subject));
    }
}