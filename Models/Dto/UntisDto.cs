using System.Text.Json.Serialization;
using NodaTime;

namespace Api.Models.Dto;

public class UntisTimetable
{
    public string Version { get; init; } = null!;
    public UntisHeaderData HeaderData { get; init; } = null!;
    public UntisMasterData MasterData { get; init; } = null!;
    public UntisTimetableData TimetableData { get; init; } = null!;
}

public class UntisHeaderData
{
    public LocalDateTime Created { get; set; }
    public string User { get; set; } = null!;
    public UntisSchoolDto School { get; set; } = null!;
    public LocalDate? SchoolYearStart { get; init; }
    public LocalDate? SchoolYearEnd { get; init; }
    public UntisFilter Filter { get; set; } = null!;
}

public class UntisFilter
{
    public LocalDateTime Start { get; init; }
    public LocalDateTime End { get; init; }
    public int Limit { get; init; } = 0;
    public int Offset { get; init; } = 0;
    public UntisFilterByChanges? FilterByChanges { get; init; }
    public List<UntisFilterByResource>? FilterByResource { get; init; }
}

public class UntisFilterByChanges
{
    public LocalDateTime? DateLastModifiedAfter { get; init; }
    public LocalDateTime? DateLastModifiedUntil { get; init; }
}

public class UntisFilterByResource
{
    public UntisResourceTypes ResourceType { get; init; } = default!;
    public long? Id { get; init; }
    public string? ExternKey { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UntisResourceTypes
{
    [JsonStringEnumMemberName("CLASS")] Class,
    [JsonStringEnumMemberName("ROOM")] Room,
    [JsonStringEnumMemberName("STUDENT")] Student,
    [JsonStringEnumMemberName("TEACHER")] Teacher,
    [JsonStringEnumMemberName("LESSON")] Lesson,
    [JsonStringEnumMemberName("PERIOD")] Period
}

public class UntisSchoolDto
{
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class UntisMasterData
{
    public List<UntisClass>? Classes { get; init; }
    public List<UntisDepartment>? Departments { get; init; }
    public List<UntisRoom>? Rooms { get; init; }
    public List<UntisStudentGroup>? StudentGroups { get; init; }
    public List<UntisStudent>? Students { get; init; }
    public List<UntisSubject>? Subjects { get; init; }
    public List<UntisTeacher>? Teachers { get; init; }
}

public class UntisClass : UntisMasterDataObject;

public class UntisDepartment : UntisMasterDataObject;

public class UntisRoom : UntisMasterDataObject;

public class UntisStudentGroup : UntisMasterDataObject;

public class UntisStudent : UntisMasterDataObject;

public class UntisSubject : UntisMasterDataObject;

public class UntisTeacher : UntisMasterDataObject;

public class UntisMasterDataObject
{
    public string Discriminator { get; init; } = null!;
    public long Id { get; init; } = 0;
    public string DisplayName { get; init; } = null!;
    public string? ExternKey { get; init; }
}

public class UntisTimetableData
{
    public List<UntisTimeGrid>? TimeGrids { get; init; }
    public List<UntisPeriod>? Periods { get; init; }
}

public class UntisTimeGrid
{
    public long Id { get; init; } = 0;
    public List<UntisTimeGridSlot>? TimeGridSlots { get; init; }
}

public class UntisTimeGridSlot
{
    public int Day { get; init; } = 0;
    public int UnitOfDay { get; init; } = 0;
    public int Start { get; init; } = 0;
    public int End { get; init; } = 0;
    public string? Name { get; init; }
}

public class UntisPeriod
{
    public long Id { get; init; } = 0;
    public LocalDateTime Modified { get; init; }
    public LocalDateTime Start { get; init; }
    public LocalDateTime End { get; init; }
    public UntisPeriodType Type { get; init; } = default!;
    public UntisPeriodStatus Status { get; init; } = default!;
    public long? RelatedPeriod { get; init; }
    public List<UntisPeriodIcon>? Icons { get; init; }
    public long? TimeGridId { get; init; }
    public UntisExam? Exam { get; init; }
    public List<UntisPeriodResource>? Departments { get; init; }
    public List<UntisPeriodResource>? Classes { get; init; }
    public List<UntisPeriodResource>? StudentGroups { get; init; }
    public List<UntisPeriodResource>? Students { get; init; }
    public List<UntisPeriodResource>? Subjects { get; init; }
    public List<UntisPeriodResource>? Rooms { get; init; }
    public List<UntisPeriodResource>? Teachers { get; init; }
}

public class UntisExam
{
    public long Id { get; set; } = 0;
    public string Type { get; set; } = null!;
    public string? Name { get; set; }
}

public class UntisPeriodResource : UntisResourceRef
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UntisResourceStatus Status { get; init; } = default!;
}

public class UntisResourceRef
{
    public string Discriminator { get; set; } = null!;
    public long Id { get; set; } = 0;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UntisPeriodType
{
    [JsonStringEnumMemberName("NORMAL_TEACHING_PERIOD")]
    NormalTeachingPeriod,

    [JsonStringEnumMemberName("ADDITIONAL_PERIOD")]
    AdditionalPeriod,

    [JsonStringEnumMemberName("STAND_BY_PERIOD")]
    StandByPeriod,

    [JsonStringEnumMemberName("OFFICE_HOUR")]
    OfficeHour,
    [JsonStringEnumMemberName("EXAM")] Exam,

    [JsonStringEnumMemberName("BREAK_SUPERVISION")]
    BreakSupervision,
    [JsonStringEnumMemberName("EVENT")] Event,
    [JsonStringEnumMemberName("MEETING")] Meeting
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UntisPeriodStatus
{
    [JsonStringEnumMemberName("REGULAR")] Regular,

    [JsonStringEnumMemberName("CANCELLED")]
    Cancelled,

    [JsonStringEnumMemberName("ADDITIONAL")]
    Additional,
    [JsonStringEnumMemberName("CHANGED")] Changed
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UntisResourceStatus
{
    [JsonStringEnumMemberName("REGULAR")] Regular,
    [JsonStringEnumMemberName("ADDED")] Added,
    [JsonStringEnumMemberName("REMOVED")] Removed
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UntisPeriodIcon
{
    [JsonStringEnumMemberName("ONLINE")] Online
}