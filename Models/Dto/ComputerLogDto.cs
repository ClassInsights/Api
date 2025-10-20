using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Api.Models.Database;
using NodaTime;

namespace Api.Models.Dto;

public record ComputerLogDto(
    [property: Description("Id of the computer log entry")]
    long? ComputerLogId,
    [property: Description("Computer the log entry belongs to")]
    long ComputerId,
    [property: Description("Timestamp of the log entry")]
    Instant Timestamp,
    [MaxLength(15)]
    [property: Description("Level of the log entry")]
    string Level,
    [MaxLength(250)]
    [property: Description("Category of the log entry")]
    string Category,
    [MaxLength(500)]
    [property: Description("Short message of the log entry")]
    string Message,
    [property: Description("Longer description of the log entry")]
    [MaxLength(5000)]
    string? Details
)
{
    public ComputerLog ToComputerLog()
    {
        return new ComputerLog()
            {
                ComputerId = ComputerId,
                Timestamp = Timestamp,
                Level = Level,
                Category = Category,
                Message = Message,
                Details = Details,
            }
        ;
    }
}