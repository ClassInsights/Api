using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Models.Database;
using Api.Models.Dto;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/")]
[ApiController]
public class UserController(IConfiguration config, IClock clock, SettingsService settingsService, ClassInsightsContext context) : ControllerBase
{
    /// <summary>
    ///     Logs user out
    /// </summary>
    /// <returns></returns>
    [HttpDelete("user")]
    public async Task<IActionResult> LogoutUser()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id))
            return Unauthorized();

        var user = await context.Users.FindAsync(id);
        if (user == null)
            return Ok();

        context.Remove(user);
        await context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("user"), AllowAnonymous]
    public async Task<IActionResult> LoginUser()
    {
        Request.Headers.TryGetValue("Authorization", out var authorization);
        var token = authorization.FirstOrDefault();
        
        if (string.IsNullOrEmpty(token))
            return Unauthorized();
        
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var response = await client.GetAsync("https://classinsights.at/api/userinfo");

        if (!response.IsSuccessStatusCode) return Unauthorized();

        var userDto = await response.Content.ReadFromJsonAsync<ApiDto.UserDto>();
        if (userDto == null)
            return Unauthorized();
        
        context.Users.Update(new User
        {
            AzureUserId = userDto.AzureUserId,
            Email = userDto.Email,
            Username = userDto.Username,
            LastSeen = clock.GetCurrentInstant()
        });

        await context.SaveChangesAsync();

        var accessToken = GenJwtToken(new ClaimsIdentity());

        return Ok(new
        {
            access_token = accessToken
        });
    }

    /// <summary>
    ///     Login endpoint for computers
    /// </summary>
    /// <returns>Jwt Bearer Token</returns>
    [HttpGet("login/computer"), AllowAnonymous]
    public async Task<IActionResult> LoginComputer()
    {
        Request.Headers.TryGetValue("Authorization", out var authorization);
        var computerToken = authorization.FirstOrDefault();
        
        if (string.IsNullOrEmpty(computerToken) || computerToken != config["ComputerToken"])
            return Unauthorized();
        
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.Role, "Computer"));

        var token = await GenJwtToken(claims);
        return token == null ? Unauthorized() : Ok(token);
    }

    private async Task<string?> GenJwtToken(ClaimsIdentity subject)
    {
        var key = await settingsService.GetSettingAsync("JwtKey");
        if (key is null)
            return null;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = DateTime.UtcNow.AddDays(2),
            Issuer = "ClassInsights",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}