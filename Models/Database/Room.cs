namespace Api.Models.Database;

public class Room
{
    public long RoomId { get; set; }

    public string? DisplayName { get; set; }

    public string? Regex { get; set; }

    public bool Enabled { get; set; }

    public virtual ICollection<Computer> Computers { get; set; } = new List<Computer>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}