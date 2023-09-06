using System.Net.WebSockets;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class ComputersController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    private readonly IMapper _mapper;

    /// <inheritdoc />
    public ComputersController(ClassInsightsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    ///     Adds or updates a Computer
    /// </summary>
    /// <param name="computer">New computer</param>
    /// <returns>
    ///     <see cref="ApiModels.Computer" />
    /// </returns>
    [HttpPost]
    [Authorize(Roles = "Computer")]
    public async Task<IActionResult> AddComputerTask(ApiModels.Computer computer)
    {
        if (HttpContext.User.Identity?.Name is { } name) computer = computer with { LastUser = name };

        var tabComputer = _mapper.Map<TabComputer>(computer);
        _context.Update(tabComputer);
        await _context.SaveChangesAsync();
        
        // map tabComputer to receive new ComputerId if it was created
        return Ok(_mapper.Map<ApiModels.Computer>(tabComputer));
    }

    /// <summary>
    ///     Get information of Computer by Name
    /// </summary>
    /// <param name="name">Name of Computer</param>
    /// <returns>
    ///     <see cref="TabComputer" />
    /// </returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetComputer(string name)
    {
        if (await _context.TabComputers.FirstOrDefaultAsync(x => x.Name == name) is { } computer)
            return Ok(_mapper.Map<ApiModels.Computer>(computer));
        return NotFound();
    }

    /// <summary>
    ///     Force PC Shutdown
    /// </summary>
    /// <param name="pcId">Id of Pc</param>
    /// <returns></returns>
    [HttpDelete("{pcId:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ShutdownComputer(int pcId)
    {
        if (!WebSocketController.PcWebSockets.TryGetValue(pcId, out var pcWebsocket))
            return NotFound();

        // check if websocket is still alive
        if (pcWebsocket.State != WebSocketState.Open)
            return NotFound();

        // send shutdown
        await pcWebsocket.SendAsync("shutdown"u8.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);
        return Ok();
    }
}