using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Models.Dto;

namespace Api.Models.Database;

[Table("room")]
public class Room
{
    [Key]
    [Column("room_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long RoomId { get; set; }

    [MaxLength(50)]
    [Column("display_name")]
    public string? DisplayName { get; set; }

    [MaxLength(100)] [Column("regex")] public string? Regex { get; set; }

    [Column("enabled")]
    [DefaultValue(false)]
    public bool Enabled { get; set; }

    public virtual ICollection<Computer> Computers { get; set; } = new List<Computer>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public RoomDto ToDto()
    {
        return new RoomDto(RoomId, Enabled, DisplayName, Regex, Computers.Count);
    }
}