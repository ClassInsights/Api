using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace Api.Controllers;

/// <inheritdoc />
[Route("api/[controller]")]
[ApiController]
public class AzureGroupsController : ControllerBase
{
    private readonly GraphServiceClient _graphClient;

    /// <inheritdoc />
    public AzureGroupsController(GraphServiceClient graphClient)
    {
        _graphClient = graphClient;
    }

    /// <summary>
    ///     Find all Azure Groups
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetAzureGroups()
    {
        var groups = await _graphClient.Groups.GetAsync(x =>
        {
            x.Options.WithAppOnly();
            x.Options.WithAuthenticationScheme("OpenIdConnect");
        });

        return Ok(groups?.Value?.Select(x => new { x.DisplayName, x.Id }));
    }
}