using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Database;

[Table("class")]
public class Class
{
    [Key]
    [Column("class_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ClassId { get; set; }

    [MaxLength(20)]
    [Column("display_name")]
    public string DisplayName { get; set; } = null!;

    [MaxLength(50)]
    [Column("azure_group_id")]
    public string? AzureGroupId { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}