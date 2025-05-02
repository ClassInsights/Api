using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Models.Dto;
using NodaTime;

namespace Api.Models.Database;

[Table("computer")]
public class Computer
{
    [Key]
    [Column("computer_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ComputerId { get; set; }

    [Column("room_id")]
    [ForeignKey("Room")]
    public long RoomId { get; set; }

    [Column("name")] [MaxLength(50)] public string Name { get; set; } = null!;

    [Column("mac_address")]
    [MaxLength(17)]
    public string? MacAddress { get; set; }

    [Column("ip_address")] [MaxLength(12)] public string? IpAddress { get; set; }

    [Column("last_seen")] public Instant LastSeen { get; set; }

    [Column("last_user")] [MaxLength(50)] public string? LastUser { get; set; }

    [Column("version")] [MaxLength(10)] public string? Version { get; set; }

    public virtual Room Room { get; set; } = null!;

    public ComputerDto ToDto()
    {
        return new ComputerDto(ComputerId, RoomId, Name, MacAddress, IpAddress, LastSeen, LastUser, Version);
    }
}