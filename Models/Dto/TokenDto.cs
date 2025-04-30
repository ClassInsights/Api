using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Api.Models.Dto;

public record TokenDto(
    [property: JsonPropertyName("access_token")]
    [property: Description("AccessToken for the user")]
    string AccessToken,
    [property: JsonPropertyName("token_type")]
    [property: Description("Type of the token")]
    string TokenType
);