using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Api.Attributes;
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
[Route("api/")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ClassInsightsContext _context;
    private readonly GraphServiceClient _graphClient;

    /// <inheritdoc />
    public UserController(IConfiguration config, ClassInsightsContext context, 
        GraphServiceClient graphClient)
    {
        _config = config;
        _context = context;
        _graphClient = graphClient;
    }

    /// <summary>
    ///     Login Endpoint for Flutter-App. User needs to be redirected to this endpoint from Microsoft OAuth2 Code Flow.
    /// </summary>
    /// <param name="code">OAuth2 Microsoft Code Flow Code</param>
    /// <returns>Jwt Bearer Token</returns>
    [HttpGet("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginByGraphCode(string code)
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
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshTokens(TokenRequest request)
    {
        var user = await _context.TabUsers.FindAsync(request.UserId);

        // validate user RefreshToken
        if (user?.Email == null || request.RefreshToken != user.RefreshToken)
            return Unauthorized();

        // get user by principalName
        var graphUser = await _graphClient.Users[user.Email].GetAsync(x =>
        {
            x.QueryParameters.Expand = new[] { "memberof" };
            x.Options.WithAppOnly();
            x.Options.WithAuthenticationScheme("OpenIdConnect");
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
    ///     Revokes the RefreshToken of the User
    /// </summary>
    /// <param name="request">
    ///     <see cref="TokenRequest" />
    /// </param>
    /// <returns></returns>
    [HttpDelete("token")]
    public async Task<IActionResult> LogoutUser(TokenRequest request)
    {
        var user = await _context.TabUsers.FindAsync(request.UserId);

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
    [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
    public async Task<IActionResult> LoginByWinAuth()
    {
        if (HttpContext.User.Identity is not WindowsIdentity { IsAuthenticated: true } identity)
            return BadRequest("You need to send NTML");

        var subjects = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, identity.Name)
        });

        if (identity.User != null && identity.User.Value is { } sid)
            subjects.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, sid));

        if (_config["Dashboard:DomainSid"] is { } domainSid &&
            ((identity.User != null && identity.User.IsEqualDomainSid(new SecurityIdentifier(domainSid))) ||
             IsLocalAttribute.IsLocalRequest(HttpContext)))
        {
            var principal = new WindowsPrincipal(identity);
            subjects.AddClaim(principal.IsInRole(new SecurityIdentifier(WellKnownSidType.AccountDomainAdminsSid,
                new SecurityIdentifier(domainSid)))
                ? new Claim(ClaimTypes.Role, "Admin")
                : new Claim(ClaimTypes.Role, "Student"));
        }
        else // check certificate if user is not in domain
        {
            if (await HttpContext.Connection.GetClientCertificateAsync() is not
                    { } clientCertificate || !ValidateClientCert(clientCertificate))
                return BadRequest("Invalid Certificate");
            subjects.AddClaim(new Claim(ClaimTypes.Role, "Guest"));
        }

        subjects.AddClaim(new Claim(ClaimTypes.Role, "Computer"));

        var token = GenJwtToken(subjects);
        return token is null ? Unauthorized() : Ok(token);
    }

    // https://stackoverflow.com/a/17225510/16871250
    private bool ValidateClientCert(X509Certificate2 certificateToValidate)
    {
        var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);

        if (_config["Dashboard:CASubject"] is not { } caSubject)
            throw new Exception("No CASubject specified!");

        var certs = store.Certificates.Find(X509FindType.FindBySubjectName, caSubject, false);
        store.Close();

        if (certs.Count < 1)
            throw new Exception("No certificate found in Store!");

        var authority = certs.First();

        var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
        chain.ChainPolicy.VerificationTime = DateTime.Now;
        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);

        chain.ChainPolicy.ExtraStore.Add(authority);

        var isChainValid = chain.Build(certificateToValidate);

        if (!isChainValid)
            return false;

        // Check if Thumbprints of Authority match
        var valid = chain.ChainElements.Any(x => x.Certificate.Thumbprint == authority.Thumbprint);
        return valid;
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
            return subjects;

        // check if user is tenant admin
        if (groups.OfType<DirectoryRole>().ToList() is { } roles &&
            roles.Any(x => x.RoleTemplateId == "62e90394-69f5-4237-9190-012177145e10"))
        {
            subjects.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
            return subjects;
        }

        // check if user is a teacher
        if (groups.Any(group => group.Id == _config["Dashboard:TeacherGroup"]))
        {
            subjects.AddClaim(new Claim(ClaimTypes.Role, "Teacher"));
            return subjects;
        }

        // return if user is no student
        var regex = _config["Dashboard:AzureGroupPattern"]?.Replace("YEAR", "[0-9]{4}").Replace("CLASS", "[A-Za-z]+");
        if (groups.Select(x => (Group)x).Where(x => Regex.IsMatch(x.DisplayName!, $"^{regex}$")).ToList() is
                not { } classGroups || classGroups is { Count: 0 })
            return subjects;

        subjects.AddClaim(new Claim(ClaimTypes.Role, "Student"));

        if (classGroups.FirstOrDefault() is not { } classGroup)
            return subjects;

        // substring of first 4 chars must be start year e.g. 2019
        // var startYear = Convert.ToInt32(classGroup.DisplayName?[..4]);

        // hypothetical school start from which grade will be calculated
        // var schoolStart = new DateTime(startYear, 9, 1);

        // always round grade up, because in 4th grade we've just been 3,.. years in school
        // var grade = Math.Round((DateTime.Now - schoolStart).TotalDays / 365.25, MidpointRounding.ToPositiveInfinity);

        // substring from char 5 must be school type, e.g. KK
        // subjects.AddClaim(new Claim("class", $"{grade}{classGroup.DisplayName?[5..]}"));

        // add id of class
        if (await _context.TabClasses.FirstOrDefaultAsync(x => x.AzureGroupId == classGroup.Id) is { } klasse)
            subjects.AddClaim(new Claim("class", klasse.ClassId.ToString()));

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

    /// <summary>
    ///     Request model for Token refresh
    /// </summary>
    /// <param name="UserId">Id of user which is associated with RefreshToken</param>
    /// <param name="RefreshToken">RefreshToken of User</param>
    public record TokenRequest(int UserId, string RefreshToken);
}