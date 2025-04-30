using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Api.Models.Database;

[Table("user")]
public class User
{
    [Key]
    [Column("user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long UserId { get; set; }

    [MaxLength(250)]
    [Column("azure_user_id")]
    public string? AzureUserId { get; set; }
    
    [Column("username")]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [Column("email")]
    [MaxLength(100)]
    public string Email { get; set; } = null!;

    [Column("last_seen")]
    public Instant? LastSeen { get; set; }
}