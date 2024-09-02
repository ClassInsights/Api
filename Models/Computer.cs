using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class Computer
{
    public int ComputerId { get; set; }

    public int RoomId { get; set; }

    public string Name { get; set; } = null!;

    public string? MacAddress { get; set; }

    public string? IpAddress { get; set; }

    public DateTime LastSeen { get; set; }

    public string? LastUser { get; set; }

    public string? Version { get; set; }

    public virtual Room Room { get; set; } = null!;
}
