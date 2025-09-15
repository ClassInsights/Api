using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Api.Models.Database;
using NodaTime;

namespace Api.Models.Dto;

public record ComputerDto(
    [property: Description("Id of the computer")]
    long ComputerId,
    [property: Description("Id of the room the computer is in")]
    long? RoomId,
    [MaxLength(50)]
    [property: Description("Name of the computer")]
    string Name,
    [MaxLength(17)]
    [property: Description("Mac address of the computer")]
    string? MacAddress,
    [MaxLength(16)]
    [property: Description("Ip address of the computer")]
    string? IpAddress,
    [property: Description("Last time the computer was active")]
    Instant LastSeen,
    [MaxLength(50)]
    [property: Description("Name of the last user that logged in on the computer")]
    string? LastUser,
    [MaxLength(10)]
    [property: Description("Version of the client installed on the computer")]
    string? Version,
    [MaxLength(2500)]
    [property: Description("Organization Unit the computer is part of")]
    string? OrganizationUnit
)
{
    [Description("Status of the computer (Online or Offline)")]
    public bool Online => LastSeen > SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromSeconds(10));

    public Computer ToComputer()
    {
        return new Computer
        {
            ComputerId = ComputerId,
            RoomId = RoomId,
            Name = Name,
            MacAddress = MacAddress,
            IpAddress = IpAddress,
            LastSeen = LastSeen,
            LastUser = LastUser,
            Version = Version,
            OrganizationUnit = OrganizationUnit
        };
    }
}

public record SendCommandDto(
    [property: Description("Command you want to send")]
    [property: MaxLength(25)]
    string Command,
    [property: Description("Id of the computer")]
    long ComputerId
);
