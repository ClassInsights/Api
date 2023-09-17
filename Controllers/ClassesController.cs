﻿using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    ///     Find class by Name
    /// </summary>
    /// <param name="name">Name of class</param>
    /// <returns>
    ///     <see cref="ApiModels.Class" />
    /// </returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetClass(string name)
    {
        if (await _context.TabClasses.FirstOrDefaultAsync(x => x.Name == name) is { } klasse)
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
    ///     Adds or updates Classes and deletes old ones
    /// </summary>
    /// <param name="classes">List of all classes</param>
    /// <returns></returns>
    [HttpPost]
    [IsLocal]
    [AllowAnonymous]
    public async Task<IActionResult> AddOrUpdateClasses(List<ApiModels.Class> classes)
    {
        if (!classes.Any()) return Ok();

        // add or update classes
        foreach (var klasse in classes)
            if (await _context.TabClasses.FindAsync(klasse.ClassId) is { } dbClass)
            {
                dbClass.Name = klasse.Name;
                dbClass.Head = klasse.Head;
                if (klasse.AzureGroupId is not null)
                {
                    if (await _context.TabAzureGroups.FindAsync(klasse.AzureGroupId) is null)
                        return NotFound($"{klasse.AzureGroupId} does not exist!");
                    dbClass.AzureGroupId = klasse.AzureGroupId;
                }

                _context.TabClasses.Update(dbClass);
            }
            else
            {
                if (klasse.AzureGroupId is null)
                {
                    // todo: microsoft api get group id
                }

                _context.TabClasses.Add(_mapper.Map<TabClass>(klasse));
            }

        // delete old classes
        var dbClasses = await _context.TabClasses.ToListAsync();
        var oldClasses = dbClasses.Where(dbClass => classes.All(c => c.ClassId != dbClass.ClassId)).ToList();
        if (oldClasses.Any())
            _context.RemoveRange(oldClasses);

        await _context.SaveChangesAsync();
        return Ok();
    }
}