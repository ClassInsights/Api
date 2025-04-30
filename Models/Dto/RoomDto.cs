using System.ComponentModel;

namespace Api.Models.Dto;

public record RoomDto(
    [property: Description("Id of the room")]
    long RoomId,
    [property: Description("Status if the room uses ClassInsights")]
    bool Enabled,
    [property: Description("Name of the room")]
    string? DisplayName,
    [property: Description("Regex to match the name of the computer")]
    string? Regex,
    [property: Description("Number of devices in the room")]
    int? DeviceCount
);