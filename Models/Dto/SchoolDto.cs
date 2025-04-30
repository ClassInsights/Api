using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Api.Models.Dto;

public record SchoolDto(
    [property: JsonPropertyName("school_id")]
    [property: Description("Id of the school")]
    long SchoolId,
    [property: Description("Name of the school")]
    string Name,
    [property: Description("Website of the school")]
    string Website,
    [property: Description("Url to the local api of the school")]
    [property: JsonPropertyName("local_api_url")]
    string LocalApiUrl,
    [property: Description("Url to the local dashboard of the school")]
    [property: JsonPropertyName("local_dashboard_url")]
    string LocalDashboardUrl,
    [property: Description("Ids of the azure groups which users are teachers")]
    [property: JsonPropertyName("azure_teacher_groups")]
    List<string> AzureTeacherGroups
);