using System.IdentityModel.Tokens.Jwt;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;
using Group = Microsoft.Graph.Models.Group;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IConfiguration _config;

    /// <inheritdoc />
    public UserController(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Login Endpoint for Flutter-App. User needs to be redirected to this endpoint from Microsoft OAuth2 Code Flow.
    /// </summary>
    /// <param name="code">OAuth2 Microsoft Code Flow Code</param>
    /// <returns>Jwt Bearer Token</returns>
    [AllowAnonymous]
    [HttpGet("login")]
    public async Task<IActionResult> LoginByCode(string code)
    {
        // connect to graph api
        var graphClient = new GraphServiceClient(
            new AuthorizationCodeCredential(
                _config["AzureAd:TenantId"],
                _config["AzureAd:ClientId"],
                _config.GetSection("AzureAd:ClientCredentials").GetChildren().First()["ClientSecret"],
                code,
                new AuthorizationCodeCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                    RedirectUri = new Uri(_config["AzureAd:RedirectUri"] ?? string.Empty),
                })
            );

        // get user object (email, username, ...)
        var me = await graphClient.Me.GetAsync();
        if (me is null)
            return BadRequest("Graph Api Me.Read refused!");

        if (me.Id is null || me.DisplayName is null || me.Mail is null)
            return BadRequest("User Id, DisplayName or Email is null!");

        // get groups in which user is in
        var groups = await graphClient.Me.MemberOf.GetAsync();
        if (groups?.Value is null)
            return BadRequest("Graph Api Me.MemberOf refused!");

        string role;
        string? className = null;

        // check if user is tenant admin
        if (groups.Value.Where(x => x is DirectoryRole) is List<DirectoryRole> roles && roles.Any(x => x.RoleTemplateId == "62e90394-69f5-4237-9190-012177145e10"))
            role = "Admin";
        else
        {
            var groupIds = groups.Value.Where(g => g.Id is not null).Select(g => g.Id!).ToList();
            if (groupIds.Contains(_config["TeacherGroup"] ?? string.Empty))
                role = "Teacher";
            else if (groups.Value.Select(x => (Group) x).Where(x => Regex.IsMatch(x.DisplayName ?? "", "^[0-9]{4}_[a-zA-Z]+$")) is { } classGroups)
            {
                role = "Student";
                if (classGroups.FirstOrDefault() is not { } classGroup)
                    return Unauthorized();

                // substring of first 4 chars must be start year e.g. 2019
                var startYear = Convert.ToInt32(classGroup.DisplayName?[..4]);

                // hypothetical school start from which grade will be calculated
                var schoolStart = new DateTime(startYear, 9, 1);
                
                // always round grade up, because in 4th grade we've just been 3,.. years in school
                var grade = Math.Round((DateTime.Now - schoolStart).TotalDays / 365.25, MidpointRounding.ToPositiveInfinity);

                // substring from char 5 must be school type, e.g. KK
                className = $"{grade}{classGroup.DisplayName?[5..]}";
            }
            else role = "Not authorized";
        }

        var subjects = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, me.DisplayName),
            new Claim(JwtRegisteredClaimNames.Sub, me.Id),
            new Claim(ClaimTypes.Email, me.Mail),
            new Claim(ClaimTypes.Role, role)
        });

        if (!string.IsNullOrEmpty(className))
        {
            subjects.AddClaim(new Claim("class", className));
        }

        var token = GenJwtToken(subjects);
        return token is null ? Unauthorized() : Ok(token);
    }

    /// <summary>
    /// Login Endpoint for Computers
    /// </summary>
    /// <returns>Jwt Bearer Token</returns>
    //[AllowAnonymous]
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    [HttpGet("pc/login")]
    public IActionResult LoginByWinAuth()
    {
        if (HttpContext.User.Identity is not WindowsIdentity { IsAuthenticated: true } identity) return BadRequest();

        var subjects = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, identity.Name),
        });

        if (identity.User?.Value is { } sid)
            subjects.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, sid));

        if (identity.User != null && _config["DomainSid"] is { } domainSid && identity.User.IsEqualDomainSid(new SecurityIdentifier(domainSid)))
            subjects.AddClaim(new Claim(ClaimTypes.Role, "Student"));
        else // // if user is not in domain then there is no user logged in on pc
            subjects.AddClaim(new Claim(ClaimTypes.Role, "Guest"));

        var token = GenJwtToken(subjects);
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
            Expires = DateTime.UtcNow.AddMinutes(10),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}