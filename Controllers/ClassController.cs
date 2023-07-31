using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClassController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    public ClassController(ClassInsightsContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> AddOrUpdateClasses(List<DbModels.TabClasses> classes)
    {
        if (!classes.Any()) return Ok();

        // add or update classes
        foreach (var c in classes)
        {
            if (await _context.TabClasses.FindAsync(c.Id) is { } dbClass)
            {
                dbClass.Name = c.Name;
                dbClass.Head = c.Head;
                if (c.Group is not null)
                {
                    if (await _context.TabGroups.FindAsync(c.Group) is null)
                        return NotFound($"{c.Group} does not exist!");
                    dbClass.Group = c.Group;
                }
                _context.TabClasses.Update(dbClass);
            }
            else
            {
                if (c.Group is null)
                {
                    // todo: microsoft api get group id
                }
                _context.TabClasses.Add(c);
            }
        }

        // delete old classes
        var dbClasses = await _context.TabClasses.ToListAsync();
        var oldClasses = dbClasses.Where(dbClass => classes.All(c => c.Id != dbClass.Id)).ToList();
        if(oldClasses.Any())
            _context.RemoveRange(oldClasses);
        
        await _context.SaveChangesAsync();
        return Ok();
    }
}