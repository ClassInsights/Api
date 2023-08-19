namespace Api.Models;

public partial class TabRoom
{
    public int RoomId { get; set; }

    public string? Name { get; set; }

    public string? LongName { get; set; }

    public virtual ICollection<TabComputer> TabComputers { get; set; } = new List<TabComputer>();

    public virtual ICollection<TabLesson> TabLessons { get; set; } = new List<TabLesson>();
}
