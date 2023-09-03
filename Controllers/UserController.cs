using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Api.Models;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
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
    private readonly ClassInsightsContext _context;

    /// <inheritdoc />
    public UserController(IConfiguration config, ClassInsightsContext context)
    {
        _config = config;
        _context = context;
    }

    /// <summary>
    ///     Login Endpoint for Flutter-App. User needs to be redirected to this endpoint from Microsoft OAuth2 Code Flow.
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
                    RedirectUri = new Uri(_config["AzureAd:RedirectUri"] ?? string.Empty)
                })
        );

        // get user object (email, username, ...)
        var me = await graphClient.Me.GetAsync(configuration =>
        {
            configuration.QueryParameters.Expand = new[] { "memberof" };
        });

        if (me?.Id is null || me.DisplayName is null || me.Mail is null)
            return BadRequest("Graph Api Me.Read refused!");

        var refreshToken = GenerateRefreshToken();

        var dbUser = new TabUser
        {
            AzureUserId = me.Id,
            Email = me.Mail,
            FirstName = me.GivenName ?? string.Empty,
            LastName = me.Surname ?? string.Empty,
            LastSeen = DateTime.Now,
            RefreshToken = refreshToken
        };

        _context.TabUsers.Update(dbUser);
        await _context.SaveChangesAsync();

        var subjects = await GetClaimsFromGraph(dbUser.UserId, me);
        var token = GenJwtToken(subjects);

        return token is null
            ? Unauthorized()
            : Ok(new
            {
                access_token = token,
                refresh_token = refreshToken
            });
    }

    /// <summary>
    ///     Generate a new Access and Refresh Token
    /// </summary>
    /// <param name="request">
    ///     <see cref="TokenRequest" />
    /// </param>
    /// <returns>New Access and Refresh Tokens</returns>
    [HttpPost]
    public async Task<IActionResult> RefreshTokens(TokenRequest request)
    {
        var user = await _context.TabUsers.FindAsync(request.UserId);

        // validate user RefreshToken
        if (user?.Email == null || request.RefreshToken != user.RefreshToken)
            return Unauthorized();

        var graphClient = new GraphServiceClient(new ClientSecretCredential(_config["AzureAd:TenantId"],
            _config["AzureAd:ClientId"],
            _config.GetSection("AzureAd:ClientCredentials").GetChildren().First()["ClientSecret"],
            new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            }));

        // get user by principalName
        var graphUser = await graphClient.Users[user.Email].GetAsync(x =>
        {
            x.QueryParameters.Expand = new []{ "memberof" };
            x.Options.WithAppOnly();
        });

        if (graphUser == null)
            return Unauthorized();

        // get new user claims
        var subjects = await GetClaimsFromGraph(user.UserId, graphUser);
        
        var token = GenJwtToken(subjects);
        var refreshToken = GenerateRefreshToken();

        // save new refreshToken to db
        user.RefreshToken = refreshToken;
        user.LastSeen = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            access_token = token,
            refresh_token = refreshToken
        });
    }

    /// <summary>
    ///     Login Endpoint for Computers
    /// </summary>
    /// <returns>Jwt Bearer Token</returns>
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    [HttpGet("login/pc")]
    public async Task<IActionResult> LoginByWinAuth()
    {
        if (HttpContext.User.Identity is not WindowsIdentity { IsAuthenticated: true } identity)
            return BadRequest();

        if (await HttpContext.Connection.GetClientCertificateAsync() is not
            { } clientCertificate /* || !clientCertificate.Verify()*/)
            return BadRequest();

        if (clientCertificate.Thumbprint != _config["CertificateThumbprint"]?.ToUpper())
            return Unauthorized("Invalid certificate");

        var subjects = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, identity.Name)
        });

        if (identity.User?.Value is { } sid)
            subjects.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, sid));

        if (identity.User != null && _config["DomainSid"] is { } domainSid &&
            identity.User.IsEqualDomainSid(new SecurityIdentifier(domainSid)))
            subjects.AddClaim(new Claim(ClaimTypes.Role, "Student"));
        else // if user is not in domain then there is no user logged in on pc
            subjects.AddClaim(new Claim(ClaimTypes.Role, "Guest"));

        var token = GenJwtToken(subjects);
        return token is null ? Unauthorized() : Ok(token);
    }

    private async Task<ClaimsIdentity> GetClaimsFromGraph(int id, User graphUser)
    {
        var subjects = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, graphUser.DisplayName!),
            new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
            new Claim(ClaimTypes.Email, graphUser.Mail!)
        });

        if (graphUser.MemberOf is not { } groups)
            return new ClaimsIdentity();

        // check if user is tenant admin
        if (groups.OfType<DirectoryRole>().ToList() is { } roles &&
            roles.Any(x => x.RoleTemplateId == "62e90394-69f5-4237-9190-012177145e10"))
        {
            subjects.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
            return subjects;
        }

        // check if user is a teacher
        if (groups.Any(group => group.Id == _config["TeacherGroup"]))
        {
            subjects.AddClaim(new Claim(ClaimTypes.Role, "Teacher"));
            return subjects;
        }

        // return if user is no student
        if (groups.Select(x => (Group)x).Where(x => Regex.IsMatch(x.DisplayName!, "^[0-9]{4}_[a-zA-Z]+$")).ToList() is
            not { } classGroups)
            return subjects;

        subjects.AddClaim(new Claim(ClaimTypes.Role, "Student"));

        if (classGroups.FirstOrDefault() is not { } classGroup)
            return subjects;

        // substring of first 4 chars must be start year e.g. 2019
        var startYear = Convert.ToInt32(classGroup.DisplayName?[..4]);

        // hypothetical school start from which grade will be calculated
        var schoolStart = new DateTime(startYear, 9, 1);

        // always round grade up, because in 4th grade we've just been 3,.. years in school
        var grade = Math.Round((DateTime.Now - schoolStart).TotalDays / 365.25, MidpointRounding.ToPositiveInfinity);

        // substring from char 5 must be school type, e.g. KK
        subjects.AddClaim(new Claim("class", $"{grade}{classGroup.DisplayName?[5..]}"));

        // add head of class
        if (await _context.TabClasses.FirstOrDefaultAsync(x => x.AzureGroupId == classGroup.Id) is { } klasse)
        {
            subjects.AddClaim(new Claim("head", klasse.Head));
        }

        return subjects;
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

    public record TokenRequest(int UserId, string RefreshToken);
}