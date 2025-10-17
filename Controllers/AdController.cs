using Api.Models.Dto;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;
using System.Net;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdController(SettingsService settingsService, ILogger<AdController> logger) : ControllerBase
{
    [HttpGet("credentials")]
    [EndpointSummary("Get credentials which are used for the Active Directory synchronization")]
    [ProducesResponseType<SettingsDto.AdCredentials>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCredentials()
    {
        var credentials = await settingsService.GetSettingAsync<SettingsDto.AdCredentials>("ad");
        
        if (credentials == null)
            return Forbid();
        
        credentials.Password = "";
        return Ok(credentials);
    }
    
    [HttpPost("credentials")]
    [EndpointSummary("Set credentials which are used for the Active Directory synchronization")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetCredentials(SettingsDto.AdCredentials credentials)
    {
        if (credentials.Password != null)
        {
            try
            {
                var identifier = new LdapDirectoryIdentifier(credentials.Domain, credentials.Port);
                var credential = new NetworkCredential(credentials.Username, credentials.Password);
                using var ldap = new LdapConnection(identifier, credential, AuthType.Basic);
                ldap.Bind();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while saving AD Creds");
                return Forbid();
            }
        }
        else
        {
            credentials.Password = (await settingsService.GetSettingAsync<SettingsDto.AdCredentials>("ad"))?.Password;
        }
        
        await settingsService.SetSettingAsync("ad", credentials);
        return Ok();
    }
    
    [HttpGet("units")]
    [EndpointSummary("Get all OUs for the configured Active Directory")]
    [ProducesResponseType<List<string>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOUs()
    {
        var credentials = await settingsService.GetSettingAsync<SettingsDto.AdCredentials>("ad");
        if (credentials == null)
            return Forbid();

        try
        {
            var identifier = new LdapDirectoryIdentifier(credentials.Domain, credentials.Port);
            var credential = new NetworkCredential(credentials.Username, credentials.Password);
            using var ldap = new LdapConnection(identifier, credential, AuthType.Basic);

            ldap.Bind();

            var namingCtxReq = new SearchRequest(
                "",
                "(objectClass=*)",
                SearchScope.Base,
                "defaultNamingContext"
            );

            var namingCtxResp = (SearchResponse)ldap.SendRequest(namingCtxReq);
            var rootDn = namingCtxResp.Entries[0].Attributes["defaultNamingContext"][0] as string;

            var ouSearchReq = new SearchRequest(
                rootDn,
                "(objectCategory=organizationalUnit)",
                SearchScope.Subtree,
                "distinguishedName"
            );
            ldap.Bind(credential);
            logger.LogInformation("OU SEARCH REQ: {RootDn}", rootDn);
            var ouSearchResp = (SearchResponse)ldap.SendRequest(ouSearchReq);

            var ouNames = (from SearchResultEntry entry in ouSearchResp.Entries
                           select entry.DistinguishedName).ToList();

            return Ok(ouNames);
        }
        catch (Exception)
        {
            return Forbid();
        }
    }
}
