using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Api.Models.Database;
using Api.Models.Dto;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace Api.Controllers;

[Route("api/")]
[ApiController]
public class UserController(
    IConfiguration config,
    IClock clock,
    IHttpClientFactory httpClientFactory,
    SettingsService settingsService,
    ClassInsightsContext context) : ControllerBase
{
    [HttpDelete("user")]
    [EndpointSummary("Logout user")]
    public async Task<IActionResult> LogoutUser()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id) || !long.TryParse(id, out var userId))
            return Unauthorized();

        var user = await context.Users.FindAsync(userId);
        if (user == null)
            return Ok();

        context.Remove(user);
        await context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("user")]
    [AllowAnonymous]
    [EndpointSummary("Login user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LoginUser([FromBody] DashboardTokenDto token)
    {
        using var client = httpClientFactory.CreateClient();

        var server = config["Server"] ?? "https://classinsights.at";
        var response = await client.PostAsJsonAsync($"{server}/api/school/dashboard/user", new
        {
            dashboard_token = token.DashboardToken
        });

        if (!response.IsSuccessStatusCode)
            return Unauthorized();

        if (await settingsService.GetSettingAsync<SchoolDto>("school") is not { } schoolDto)
            return NotFound();

        var userDto = await response.Content.ReadFromJsonAsync<UserDto>(JsonSerializerOptions.Web);
        var userSchoolDto = userDto?.Schools.FirstOrDefault(s => s.SchoolId == schoolDto.SchoolId);

        if (userSchoolDto == null ||
            (!userSchoolDto.Roles.Contains("Admin") && !userSchoolDto.Roles.Contains("Teacher")))
            return Unauthorized();

        var user = context.Users.Update(new User
        {
            AzureUserId = userDto!.AzureUserId,
            Email = userDto.Email,
            Username = userDto.Username,
            LastSeen = clock.GetCurrentInstant()
        });

        await context.SaveChangesAsync();
        var claims = new ClaimsIdentity();

        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Entity.UserId.ToString()));
        claims.AddClaim(new Claim(JwtRegisteredClaimNames.Name, userDto.Username));
        claims.AddClaim(new Claim(JwtRegisteredClaimNames.Email, userDto.Email));
        claims.AddClaim(new Claim("school_name", schoolDto.Name));
        claims.AddClaim(new Claim(ClaimTypes.Role, JsonSerializer.Serialize(userSchoolDto.Roles),
            JsonClaimValueTypes.JsonArray));

        return Ok(new TokenDto(GenJwtToken(claims), "Bearer"));
    }

    [HttpPost("login/computer")]
    [AllowAnonymous]
    [EndpointSummary("Login endpoint for computers")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult LoginComputer([FromBody] ComputerTokenDto tokenDto)
    {
        if (tokenDto.ComputerToken != config["ComputerToken"])
            return Unauthorized();

        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.Role, "Computer"));

        var token = GenJwtToken(claims);
        return Ok(token);
    }

    private string GenJwtToken(ClaimsIdentity subject)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = DateTime.UtcNow.AddDays(2),
            Issuer = "ClassInsights",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtKey"]!)),
                SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}