using System;
using System.Collections.Generic;
using NodaTime;

namespace Api.Models.Database;

public partial class Computer
{
    public long ComputerId { get; set; }

    public long RoomId { get; set; }

    public string Name { get; set; } = null!;

    public string? MacAddress { get; set; }

    public string? IpAddress { get; set; }

    public Instant LastSeen { get; set; }

    public string? LastUser { get; set; }

    public string? Version { get; set; }

    public virtual Room Room { get; set; } = null!;
}
