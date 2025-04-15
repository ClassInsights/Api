using System.Text.Json.Serialization;

namespace Api.Models.Dto;

public class ServerDto
{
    public record UserDto (
        [property: JsonPropertyName("session_id")]
        long SessionId,
        string Username,
        [property: JsonPropertyName("azure_user_id")]
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
        string Website,
        [property: JsonPropertyName("local_api_url")]
        string LocalApiUrl,
        [property: JsonPropertyName("local_dashboard_url")]
        string LocalDashboardUrl,
        [property: JsonPropertyName("azure_teacher_groups")]
        List<string> AzureTeacherGroups
    );
}