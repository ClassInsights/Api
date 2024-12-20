﻿using System.Net.WebSockets;
using System.Text;
using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

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
    /// <param name="computerDto">New computer</param>
    /// <returns>
    ///     <see crefApiDto.ComputerDtoer" />
    /// </returns>
    [HttpPost]
    [Authorize(Roles = "Computer")]
    public async Task<IActionResult> UpdateComputer(ApiDto.ComputerDto computerDto)
    {
        if (HttpContext.User.Identity?.Name is { } name) computerDto.LastUser = name;
        var tabComputer = _mapper.Map<Computer>(computerDto);
        _context.Update(tabComputer);
        await _context.SaveChangesAsync();

        // map tabComputer to receive new ComputerId if it was created
        return Ok(_mapper.Map<ApiDto.ComputerDto>(tabComputer));
    }

    /// <summary>
    ///     Get information of Computer by Name
    /// </summary>
    /// <param name="name">Name of Computer</param>
    /// <returns>
    ///     <see cref="Computer" />
    /// </returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetComputer(string name)
    {
        if (await _context.Computers.FirstOrDefaultAsync(x => x.Name.Contains(name)) is { } computer)
            return Ok(_mapper.Map<ApiDto.ComputerDto>(computer));
        return NotFound();
    }

    /// <summary>
    ///     Send command to Computer
    /// </summary>
    /// <param name="computerId">Id of Computer</param>
    /// <param name="command">Action which Computer should perform (shutdown, restart, logoff)</param>
    /// <returns></returns>
    [HttpPatch("{computerId:int}/{command}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> SendCommand(int computerId, string command)
    {
        if (!WebSocketController.ComputerWebSockets.TryGetValue(computerId, out var pcWebsocket))
            return NotFound();

        // check if websocket is still alive
        if (pcWebsocket.State != WebSocketState.Open)
            return NotFound();

        // send command
        await pcWebsocket.SendAsync(Encoding.UTF8.GetBytes(command).ToArray(), WebSocketMessageType.Text, true,
            CancellationToken.None);

        var computer = await _context.Computers.FindAsync(computerId);
        _context.Logs.Add(new Log
        {
            Message = $"Send {command} to '{computer?.Name}' (Id: {computerId})",
            Username = HttpContext.User.FindFirst("name")?.Value ?? "No username found in Token",
            Date = SystemClock.Instance.GetCurrentInstant()
        });
        await _context.SaveChangesAsync();
        return Ok();
    }
}