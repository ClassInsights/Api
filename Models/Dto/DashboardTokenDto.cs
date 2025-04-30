using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Api.Models.Dto;

public record DashboardTokenDto(
    [property: JsonPropertyName("dashboard_token")]
    [property: Description("Token from official classinsights website redirect")]
    string DashboardToken
);