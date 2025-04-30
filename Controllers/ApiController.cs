using System.ComponentModel;
using System.Reflection;
using Api.Models.Dto;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api")]
[ApiController]
public class ApiController(ILogger<ApiController> logger) : ControllerBase
{
    private const string UpdaterImage = "ghcr.io/classinsights/updater:latest";
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    [HttpGet]
    [EndpointSummary("Get API information")]
    [Description("Retruns the current version and platform of where the API is running")]
    [ProducesResponseType<ApiVersionDto>(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new ApiVersionDto(Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
            Environment.OSVersion.Platform.ToString()));
    }

    [HttpPost("update")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Update the API and Dashboard")]
    public async Task<IActionResult> Update()
    {
        await Semaphore.WaitAsync();
        try
        {
            var client = new DockerClientConfiguration().CreateClient();

            logger.LogInformation("Pull updater image");
            await client.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = UpdaterImage
            }, new AuthConfig(), new Progress<JSONMessage>(msg => logger.LogInformation("{Message}", msg.Status)));

            logger.LogInformation("Create updater container");
            var created = await client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = UpdaterImage,
                HostConfig = new HostConfig
                {
                    Binds = new List<string>
                    {
                        "/var/run/docker.sock:/var/run/docker.sock"
                    }
                }
            });

            logger.LogInformation("Start updater container");
            await client.Containers.StartContainerAsync(created.ID, new ContainerStartParameters());
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while updating");
            return StatusCode(500);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}