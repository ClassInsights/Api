namespace Api;

/// <summary>
///     DTO Models for Api requests and responses
/// </summary>
public class ApiModels
{
    /// <summary>
    ///     Class object
    /// </summary>
    /// <param name="ClassId">Id of Class</param>
    /// <param name="Name">Name of Class</param>
    /// <param name="Head">Head of Class</param>
    /// <param name="AzureGroupId">Id of Group in AzureAd</param>
    public record Class(int ClassId, string Name, string Head, string? AzureGroupId);

    /// <summary>
    ///     Lesson object
    /// </summary>
    /// <param name="LessonId">Id of Lesson</param>
    /// <param name="RoomId">Id of Room</param>
    /// <param name="SubjectId">Id of Subject</param>
    /// <param name="ClassId">Id of Class</param>
    /// <param name="StartTime">Lesson begin</param>
    /// <param name="EndTime">Lesson end</param>
    public record Lesson(int LessonId, int RoomId, int SubjectId, int ClassId, DateTime? StartTime, DateTime? EndTime);

    /// <summary>
    ///     Computer object
    /// </summary>
    /// <param name="ComputerId">Id of Computer</param>
    /// <param name="RoomId">Id of Room</param>
    /// <param name="Name">Name of Computer</param>
    /// <param name="MacAddress">MAC Address of Computer</param>
    /// <param name="IpAddress">IP Address of Computer</param>
    /// <param name="LastUser">Last signed in User</param>
    /// <param name="LastSeen">Last time of Heartbeat</param>
    public record Computer(int ComputerId, int RoomId, string Name, string MacAddress, string IpAddress,
        string? LastUser, DateTime LastSeen);

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
    public record Subject(int SubjectId, string Name, string LongName);
}