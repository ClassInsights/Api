using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    public TestController(ClassInsightsContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetTest()
    {
        var users = await _context.TabUsers.ToListAsync();
        return Ok(users);
    }
}