using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Api.Models.Dto;

public record UserDto(
    [property: JsonPropertyName("session_id")]
    [property: Description("Id of the session")]
    long SessionId,
    [property: Description("Username of the user")]
    string Username,
    [property: Description("Id of the user in azure")]
    [property: JsonPropertyName("azure_user_id")]
    string AzureUserId,
    [property: Description("Email of the user")]
    string Email,
    [property: Description("Schools the user is in")]
    UserSchoolDto[] Schools
);

public record UserSchoolDto(
    [property: Description("Id of the school")]
    long SchoolId,
    [property: Description("Roles of the user in the school")]
    string[] Roles
);