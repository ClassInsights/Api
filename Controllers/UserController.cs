﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public async Task<IActionResult> LoginByCode(string code)
    {
       /* var scopes = new[] { "User.Read" };
        var tenantId = _configuration.GetValue<string>("AzureAd:TenantId");
        var clientId = _configuration.GetValue<string>("AzureAd:ClientId");
        var clientSecret = _configuration.GetSection("AzureAd:ClientCredentials").GetChildren().First().GetValue<string>("ClientSecret");

        var options = new AuthorizationCodeCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            RedirectUri = new Uri(""),
        };

        var authCodeCredential = new AuthorizationCodeCredential(tenantId, clientId, clientSecret, code, options);
        var graphClient = new GraphServiceClient(authCodeCredential, scopes);*/

       var issuer = _configuration["Jwt:Issuer"];
       var audience = _configuration["Jwt:Audience"];
       var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

       var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

       var subject = new ClaimsIdentity(new[]
       {
           new Claim(JwtRegisteredClaimNames.Sub, "Julian"),
           new Claim(JwtRegisteredClaimNames.Email, "info@versahl.at"),
       });

       var expires = DateTime.UtcNow.AddMinutes(10);
       var tokenDescriptor = new SecurityTokenDescriptor
       {
           Subject = subject,
           Expires = expires,
           Issuer = issuer,
           Audience = audience,
           SigningCredentials = signingCredentials
       };

       var tokenHandler = new JwtSecurityTokenHandler();
       var token = tokenHandler.CreateToken(tokenDescriptor);
       var jwtToken = tokenHandler.WriteToken(token);

       return Ok(jwtToken);
    }
}