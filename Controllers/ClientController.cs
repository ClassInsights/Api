using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientController(IHttpClientFactory httpClientFactory, ILogger<ClientController> logger) : ControllerBase
{
    private const string ClientVersion = "1.0.0.1";
    private const string DownloadBaseUrl = "https://github.com/ClassInsights/WinService/releases/download";

    [HttpGet("version")]
    [EndpointSummary("Get supported client version")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            client_version = ClientVersion
        });
    }

    [HttpGet("download")]
    [EndpointSummary("Download latest, supported client installer")]
    public async Task<IActionResult> GetLatestVersion(CancellationToken cancellationToken)
    {
        const string fileName = $"ClassInsights_{ClientVersion}.msi";
        const string downloadUrl = $"{DownloadBaseUrl}/{ClientVersion}/ClassInsights.msi";
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        if (System.IO.File.Exists(filePath))
        {
            logger.LogInformation("Serving saved file: {FilePath}", filePath);
            return PhysicalFile(filePath, MediaTypeNames.Application.Octet, fileName);
        }

        try
        {
            logger.LogInformation("Downloading file from {DownloadUrl}", downloadUrl);
            using var httpClient = httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(downloadUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to download file. Status code: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, "Error downloading file.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(fileStream, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while downloading or saving the file.");
            return StatusCode(500, "An error occurred while processing the request.");
        }

        return PhysicalFile(filePath, MediaTypeNames.Application.Octet, fileName);
    }
}