﻿using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class TabComputer
{
    public int ComputerId { get; set; }

    public int RoomId { get; set; }

    public string Name { get; set; } = null!;

    public string? MacAddress { get; set; }

    public string? IpAddress { get; set; }

    public DateTime LastSeen { get; set; }

    public string? LastUser { get; set; }

    public string? Version { get; set; }

    public virtual TabRoom Room { get; set; } = null!;
}
