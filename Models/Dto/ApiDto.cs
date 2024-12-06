namespace Api.Models.Dto;

/// <summary>
///     DTO Models for Api requests and responses
/// </summary>
public class ApiDto
{
    /// <summary>
    ///     Class object
    /// </summary>
    /// <param name="ClassId">Id of Class</param>
    /// <param name="Name">Name of Class</param>
    /// <param name="Head">Head of Class</param>
    /// <param name="AzureGroupId">Id of Group in AzureAd</param>
    public class ClassDto
    {
        /// <summary>Id of Class</summary>
        public int? ClassId { get; set; }

        /// <summary>Name of Class</summary>
        public string? Name { get; set; }

        /// <summary>Head of Class</summary>
        public string? Head { get; set; }

        /// <summary>Id of Group in AzureAd</summary>
        public string? AzureGroupId { get; set; }
    }

    /// <summary>
    ///     Lesson object
    /// </summary>
    /// <param name="LessonId">Id of Lesson</param>
    /// <param name="RoomId">Id of Room</param>
    /// <param name="SubjectId">Id of Subject</param>
    /// <param name="ClassId">Id of Class</param>
    /// <param name="StartTime">Lesson begin</param>
    /// <param name="EndTime">Lesson end</param>
    public record LessonDto(int LessonId, int RoomId, int SubjectId, int ClassId, DateTime? StartTime, DateTime? EndTime);

    /// <summary>
    ///     Computer object
    /// </summary>
    public class ComputerDto
    {
        /// <summary>Id of Computer</summary>
        public int ComputerId { get; set; }

        /// <summary>Id of Room</summary>
        public int RoomId { get; set; }

        /// <summary>Name of Computer</summary>
        public string Name { get; set; }

        /// <summary>MAC Address of Computer</summary>
        public string MacAddress { get; set; }

        /// <summary>IP Address of Computer</summary>
        public string IpAddress { get; set; }

        /// <summary>Last signed in User</summary>
        public string? LastUser { get; set; }

        /// <summary>Last time of Heartbeat</summary>
        public DateTime LastSeen { get; set; }
        
        /// <summary>Version of ClassInsights Client</summary>
        public string? Version { get; set; }

        /// <summary>
        /// Online state of Computer
        /// </summary>
        public bool Online => LastSeen > DateTime.Now.AddSeconds(-10);
    }

    /// <summary>
    ///     Room object
    /// </summary>
    /// <param name="RoomId">Id of Room</param>
    /// <param name="Name">Name of Room</param>
    /// <param name="LongName">Fullname of Room</param>
    /// <param name="DeviceCount">Count of Computers inside of Room</param>
    public record RoomDto(long RoomId, string DisplayName, int? DeviceCount);

    /// <summary>
    ///     Subject object
    /// </summary>
    /// <param name="SubjectId">Id of Subject</param>
    /// <param name="Name">Name of Subject</param>
    /// <param name="LongName">Fullname of Subject</param>
    public record SubjectDto(int SubjectId, string Name, string LongName);

    /// <summary>
    ///     SchoolYear object
    /// </summary>
    public class SchoolYearDto
    {
        /// <summary>
        ///     SchoolYear object
        /// </summary>
        /// <param name="name">Name of SchoolYear</param>
        /// <param name="startDate">StartDate of SchoolYear</param>
        /// <param name="endDate">EndDate of SchoolYear</param>
        public SchoolYearDto(string name, DateTime startDate, DateTime endDate)
        {
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
        }

        /// <summary>Name of SchoolYear</summary>
        public string Name { get; set; }

        /// <summary>StartDate of SchoolYear</summary>
        public DateTime StartDate { get; set; }

        /// <summary>EndDate of SchoolYear</summary>
        public DateTime EndDate { get; set; }
    }
}