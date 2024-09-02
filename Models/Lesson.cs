using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int RoomId { get; set; }

    public int SubjectId { get; set; }

    public int ClassId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
