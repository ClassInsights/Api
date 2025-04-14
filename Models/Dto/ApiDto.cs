using System.Text.Json.Serialization;
using NodaTime;

namespace Api.Models.Dto;

/// <summary>
///     DTO Models for Api requests and responses
/// </summary>
public class ApiDto
{
    public class DashboardTokenDto(string dashboardToken)
    {
        [JsonPropertyName("dashboard_token")]
        public string DashboardToken { get; } = dashboardToken;
    }
    
    public class ComputerTokenDto(string computerToken)
    {
        [JsonPropertyName("computer_token")]
        public string ComputerToken { get; } = computerToken;
    }
    
    public class ClassDto
    {
        public long? ClassId { get; set; }
        public string? DisplayName { get; set; }
        public string? AzureGroupId { get; set; }
    }
    
    public class ComputerDto
    {
        public long ComputerId { get; set; }
        public long RoomId { get; set; }
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public string? LastUser { get; set; } 
        public Instant LastSeen { get; set; }
        public string? Version { get; set; }
        public bool Online => LastSeen > SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromSeconds(10));
    }
    
    public record RoomDto(long RoomId, string DisplayName, string Regex, bool Enabled, int? DeviceCount);
    public record SubjectDto(long SubjectId, string DisplayName);
    public record LessonDto(long LessonId, long RoomId, long SubjectId, long ClassId, Instant? Start, Instant? End);
}