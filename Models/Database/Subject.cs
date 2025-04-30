namespace Api.Models.Database;

public class Subject
{
    public long SubjectId { get; set; }

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}