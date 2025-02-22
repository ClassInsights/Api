using System.Text.Json.Serialization;

namespace Api.Models.Dto;

public class ServerDto
{
    public record UserDto (
        string SessionId,
        string Username,
        string AzureUserId,
        string Email,
        UserSchoolDto[] Schools
    );
    
    public record UserSchoolDto (
        long SchoolId,
        string[] Roles
    );

    public record SchoolDto(
        [property: JsonPropertyName("school_id")]
        long SchoolId,
        string Name,
        [property: JsonPropertyName("local_api_url")]
        string LocalApiUrl,
        [property: JsonPropertyName("local_dashboard_url")]
        string LocalDashboardUrl,
        [property: JsonPropertyName("azure_teacher_groups")]
        List<string> AzureTeacherGroups
    );
}