using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Api.Models.Dto;

public record ComputerTokenDto(
    [property: JsonPropertyName("computer_token")]
    [property: Description("AccessToken for the computer")]
    string ComputerToken
);