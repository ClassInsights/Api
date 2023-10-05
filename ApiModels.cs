namespace Api;

/// <summary>
///     DTO Models for Api requests and responses
/// </summary>
public abstract class ApiModels
{
    /// <summary>
    ///     Class object
    /// </summary>
    /// <param name="ClassId">Id of Class</param>
    /// <param name="Name">Name of Class</param>
    /// <param name="Head">Head of Class</param>
    /// <param name="AzureGroupId">Id of Group in AzureAd</param>
    public abstract record Class(int ClassId, string Name, string Head, string? AzureGroupId);

    /// <summary>
    ///     Lesson object
    /// </summary>
    /// <param name="LessonId">Id of Lesson</param>
    /// <param name="RoomId">Id of Room</param>
    /// <param name="SubjectId">Id of Subject</param>
    /// <param name="ClassId">Id of Class</param>
    /// <param name="StartTime">Lesson begin</param>
    /// <param name="EndTime">Lesson end</param>
    public abstract record Lesson(int LessonId, int RoomId, int SubjectId, int ClassId, DateTime? StartTime, DateTime? EndTime);

    /// <summary>
    ///     Computer object
    /// </summary>
    public class Computer
    {
        /// <summary>
        ///     Computer object
        /// </summary>
        /// <param name="computerId">Id of Computer</param>
        /// <param name="roomId">Id of Room</param>
        /// <param name="name">Name of Computer</param>
        /// <param name="macAddress">MAC Address of Computer</param>
        /// <param name="ipAddress">IP Address of Computer</param>
        /// <param name="lastUser">Last signed in User</param>
        /// <param name="lastSeen">Last time of Heartbeat</param>
        public Computer(int computerId, int roomId, string name, string macAddress, string ipAddress,
            string? lastUser, DateTime lastSeen)
        {
            ComputerId = computerId;
            RoomId = roomId;
            Name = name;
            MacAddress = macAddress;
            IpAddress = ipAddress;
            LastUser = lastUser;
            LastSeen = lastSeen;
        }

        /// <summary>Id of Computer</summary>
        public int ComputerId { get; }

        /// <summary>Id of Room</summary>
        public int RoomId { get; }

        /// <summary>Name of Computer</summary>
        public string Name { get; }

        /// <summary>MAC Address of Computer</summary>
        public string MacAddress { get; }

        /// <summary>IP Address of Computer</summary>
        public string IpAddress { get; }

        /// <summary>Last signed in User</summary>
        public string? LastUser { get; set; }

        /// <summary>Last time of Heartbeat</summary>
        private DateTime LastSeen { get; }

        /// <summary>
        /// Returns the Online Status of a Computer
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
    public record Room(int RoomId, string Name, string LongName, int? DeviceCount);

    /// <summary>
    ///     Subject object
    /// </summary>
    /// <param name="SubjectId">Id of Subject</param>
    /// <param name="Name">Name of Subject</param>
    /// <param name="LongName">Fullname of Subject</param>
    public abstract record Subject(int SubjectId, string Name, string LongName);

    /// <summary>
    ///     SchoolYear object
    /// </summary>
    public class SchoolYear
    {
        /// <summary>
        ///     SchoolYear object
        /// </summary>
        /// <param name="name">Name of SchoolYear</param>
        /// <param name="startDate">StartDate of SchoolYear</param>
        /// <param name="endDate">EndDate of SchoolYear</param>
        public SchoolYear(string name, DateTime startDate, DateTime endDate)
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