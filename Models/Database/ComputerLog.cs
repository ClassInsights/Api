using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Models.Dto;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Api.Models.Database;

[Table("computer_log")]
[Index(nameof(ComputerId), nameof(Timestamp))]
public class ComputerLog
{
    [Key]
    [Column("computer_log_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ComputerLogId { get; set; }

    [Required]
    [Column("computer_id")]
    [ForeignKey("Computer")]
    public long ComputerId { get; set; }

    [Required]
    [Column("timestamp")]
    public Instant Timestamp { get; set; }

    [Required]
    [Column("level")]
    [MaxLength(15)]
    public string Level { get; set; } = null!;
    
    [Required]
    [Column("category")]
    [MaxLength(250)]
    public string Category { get; set; } = null!;

    [Required]
    [Column("message")]
    [MaxLength(250)]
    public string Message { get; set; } = null!;

    [Column("details")]
    [MaxLength(1500)]
    public string? Details { get; set; }
    
    public virtual Computer Computer { get; set; } = null!;
    
    public ComputerLogDto ToDto()
    {
        return new ComputerLogDto(ComputerLogId, ComputerId, Timestamp, Level, Category, Message, Details);
    }
}