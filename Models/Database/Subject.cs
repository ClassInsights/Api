using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Models.Dto;

namespace Api.Models.Database;

[Table("subject")]
public class Subject
{
    [Key]
    [Column("subject_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long SubjectId { get; set; }

    [MaxLength(50)]
    [Column("display_name")]
    public string DisplayName { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public SubjectDto ToDto()
    {
        return new SubjectDto(SubjectId, DisplayName);
    }
}