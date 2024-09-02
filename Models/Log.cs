using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class Log
{
    public long LogId { get; set; }

    public string Message { get; set; } = null!;

    public string Username { get; set; } = null!;

    public DateTime Date { get; set; }
}
