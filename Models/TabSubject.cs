namespace Api.Models;

public partial class TabSubject
{
    public int SubjectId { get; set; }

    public string Name { get; set; } = null!;

    public string LongName { get; set; } = null!;

    public virtual ICollection<TabLesson> TabLessons { get; set; } = new List<TabLesson>();
}
