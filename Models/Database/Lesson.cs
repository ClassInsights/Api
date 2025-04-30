using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Api.Models.Database;

[Table("lesson")]
public class Lesson
{
    [Key]
    [Column("lesson_id")]
    public long LessonId { get; set; }
    
    [Column("room_id")]
    [ForeignKey("Room")]
    public long RoomId { get; set; }

    [Column("subject_id")]
    [ForeignKey("Subject")]
    public long SubjectId { get; set; }

    [Column("class_id")]
    [ForeignKey("Class")]
    public long ClassId { get; set; }

    [Column("start")]
    public Instant? Start { get; set; }

    [Column("end")]
    public Instant? End { get; set; }

    [Column("period_id")]
    public long PeriodId { get; set; }

    public virtual Class Class { get; set; } = null!;
    public virtual Room Room { get; set; } = null!;
    public virtual Subject Subject { get; set; } = null!;
}