﻿using NodaTime;

namespace Api.Models.Database;

public class Lesson
{
    public long LessonId { get; set; }

    public long RoomId { get; set; }

    public long SubjectId { get; set; }

    public long ClassId { get; set; }

    public Instant? Start { get; set; }

    public Instant? End { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}