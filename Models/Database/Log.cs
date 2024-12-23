using NodaTime;

namespace Api.Models.Database;

public class Log
{
    public long LogId { get; set; }

    public string Message { get; set; } = null!;

    public string Username { get; set; } = null!;

    public Instant Date { get; set; }
}