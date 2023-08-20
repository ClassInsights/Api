﻿using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComputersController : ControllerBase
{
    private readonly ClassInsightsContext _context;
    private readonly IMapper _mapper;

    public ComputersController(ClassInsightsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Is used to add or update an existing computer
    /// </summary>
    /// <param name="computer">Heartbeat object with the newest data from a computer</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> AddComputerTask(ApiModels.Computer computer)
    {
        if (await _context.TabComputers.FindAsync(computer.ComputerId) is { } pc)
        {
            pc.Name = computer.Name;
            pc.MacAddress = computer.MacAddress;
            pc.LastSeen = computer.LastSeen;
            pc.RoomId = computer.RoomId;
            pc.IpAddress = computer.IpAddress;
        }
        else await _context.TabComputers.AddAsync(_mapper.Map<TabComputer>(computer));

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<ApiModels.Computer>(computer));
    }

    /// <summary>
    /// Get information of Computer by Name
    /// </summary>
    /// <param name="name">Name of computer</param>
    /// <returns><see cref="TabComputer"/></returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetComputer(string name)
    {
        if (await _context.TabComputers.FirstOrDefaultAsync(x => x.Name == name) is { } computer)
            return Ok(_mapper.Map<ApiModels.Computer>(computer));
        return NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> ShutdownComputer(int id)
    {
        //var user = WindowsIdentity.GetCurrent().User; // PGI\julian
        //var claims = HttpContext.User.Claims;
        //var a = HttpContext.User.Identity.Name;

        return Ok();
    }
}