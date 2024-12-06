using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Models.Database;
using Api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ClassInsightsContext _context;

    /// <inheritdoc />
    public UserController(IConfiguration config, ClassInsightsContext context)
    {
        _config = config;
        _context = context;
    }

    /// <summary>
    ///     Revokes the RefreshToken of the User
    /// </summary>
    /// <param name="request">
    ///     <see cref="TokenRequest" />
    /// </param>
    /// <returns></returns>
    [HttpDelete("token")]
    public async Task<IActionResult> LogoutUser(TokenRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);

        // validate user RefreshToken
        if (user == null || request.RefreshToken != user.RefreshToken)
            return Unauthorized();

        _context.Remove(user);
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    ///     Login Endpoint for Computers
    /// </summary>
    /// <returns>Jwt Bearer Token</returns>
    [HttpGet("login/pc")]
    public async Task<IActionResult> LoginComputers()
    {
        if (await HttpContext.Connection.GetClientCertificateAsync() is not
                { } clientCertificate || !CertificateUtils.ValidateClientCertificate(clientCertificate))
            return BadRequest("Invalid Client Certificate");
        
        var claims = new ClaimsIdentity();
        var token = GenJwtToken(claims);
        return token is null ? Unauthorized() : Ok(token);
    }

    private string? GenJwtToken(ClaimsIdentity subject)
    {
        var key = _config["Jwt:Key"];
        if (key is null)
            return null;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = DateTime.UtcNow.AddDays(2),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    ///     Request model for Token refresh
    /// </summary>
    /// <param name="UserId">Id of user which is associated with RefreshToken</param>
    /// <param name="RefreshToken">RefreshToken of User</param>
    public record TokenRequest(int UserId, string RefreshToken);
}