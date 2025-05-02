using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Api.Models.Database;

[Table("log")]
public class Log
{
    [Key]
    [Column("log_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long LogId { get; set; }

    [Column("message")] [MaxLength(200)] public string Message { get; set; } = null!;

    [Column("username")] [MaxLength(50)] public string Username { get; set; } = null!;

    [Column("date")] public Instant Date { get; set; }
}