using System;
using System.Collections.Generic;
using NodaTime;

namespace Api.Models.Database;

public partial class Log
{
    public long LogId { get; set; }

    public string Message { get; set; } = null!;

    public string Username { get; set; } = null!;

    public Instant Date { get; set; }
}
