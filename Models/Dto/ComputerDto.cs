using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Api.Models.Dto;

public record ComputerDto(
    [property: Description("Id of the computer")]
    long ComputerId,
    [property: Description("Id of the room the computer is in")]
    long RoomId,
    [property: MaxLength(50)]
    [property: Description("Name of the computer")]
    string Name,
    [property: MaxLength(50)]
    [property: Description("Mac address of the computer")]
    string? MacAddress,
    [property: MaxLength(50)]
    [property: Description("Ip address of the computer")]
    string? IpAddress,
    [property: Description("Last time the computer was active")]
    Instant LastSeen,
    [property: MaxLength(75)]
    [property: Description("Name of the last user that logged in on the computer")]
    string? LastUser,
    [property: MaxLength(20)]
    [property: Description("Version of the client installed on the computer")]
    string? Version
)
{
    [Description("Status of the computer (Online or Offline)")]
    public bool Online => LastSeen > SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromSeconds(10));
}

public record SendCommandDto(
    [property: Description("Command you want to send")]
    [property: MaxLength(25)]
    string Command,
    [property: Description("Id of the computer")]
    long ComputerId
);