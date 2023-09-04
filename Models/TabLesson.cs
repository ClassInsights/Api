namespace Api.Models;

public class TabLesson
{
    public int LessonId { get; set; }

    public int RoomId { get; set; }

    public int SubjectId { get; set; }

    public int ClassId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual TabClass Class { get; set; } = null!;

    public virtual TabRoom Room { get; set; } = null!;

    public virtual TabSubject Subject { get; set; } = null!;
}