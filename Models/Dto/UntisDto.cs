using System.Text.Json.Serialization;
using NodaTime;

namespace Api.Models.Dto;

public class TokenDto
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;

    [JsonPropertyName("token_type")] public string TokenType { get; set; } = null!;
}

public class Error
{
    public string Title { get; set; } = null!;
    public int Status { get; set; } = default!;
    public string? Detail { get; set; }
}

public class TimetableDto
{
    public string Version { get; set; } = null!;
    public HeaderDataDto HeaderData { get; set; } = null!;
    public MasterDataDto MasterData { get; set; } = null!;
    public TimetableDataDto TimetableData { get; set; } = null!;
}

public class HeaderDataDto
{
    public LocalDateTime Created { get; set; }
    public string User { get; set; } = null!;
    public UntisSchoolDto School { get; set; } = null!;
    public LocalDate? SchoolYearStart { get; set; }
    public LocalDate? SchoolYearEnd { get; set; }
    public FilterDto Filter { get; set; } = null!;
}

public class FilterDto
{
    public LocalDateTime Start { get; set; }
    public LocalDateTime End { get; set; }
    public int Limit { get; set; } = default!;
    public int Offset { get; set; } = default!;
    public FilterByChangesDto? FilterByChanges { get; set; }
    public List<FilterByResourceDto>? FilterByResource { get; set; }
}

public class FilterByChangesDto
{
    public LocalDateTime? DateLastModifiedAfter { get; set; }
    public LocalDateTime? DateLastModifiedUntil { get; set; }
}

public class FilterByResourceDto
{
    public FilterResourceTypeEnum ResourceType { get; set; } = default!;
    public long? Id { get; set; }
    public string? ExternKey { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FilterResourceTypeEnum
{
    CLASS,
    ROOM,
    STUDENT,
    TEACHER,
    LESSON,
    PERIOD
}

public class UntisSchoolDto
{
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class MasterDataDto
{
    public List<ClassDto>? Classes { get; set; }
    public List<DepartmentDto>? Departments { get; set; }
    public List<RoomDto>? Rooms { get; set; }
    public List<StudentGroupDto>? StudentGroups { get; set; }
    public List<StudentDto>? Students { get; set; }
    public List<SubjectDto>? Subjects { get; set; }
    public List<TeacherDto>? Teachers { get; set; }
}

public class ClassDto : MasterDataObjectDto
{
}

public class DepartmentDto : MasterDataObjectDto
{
}

public class RoomDto : MasterDataObjectDto
{
}

public class StudentGroupDto : MasterDataObjectDto
{
}

public class StudentDto : MasterDataObjectDto
{
}

public class SubjectDto : MasterDataObjectDto
{
}

public class TeacherDto : MasterDataObjectDto
{
}

public class MasterDataObjectDto
{
    public string Discriminator { get; set; } = null!;
    public long Id { get; set; } = default!;
    public string DisplayName { get; set; } = null!;
    public string? ExternKey { get; set; }
}

public class TimetableDataDto
{
    public List<TimeGridDto>? TimeGrids { get; set; }
    public List<PeriodDto>? Periods { get; set; }
}

public class TimeGridDto
{
    public long Id { get; set; } = default!;
    public List<TimeGridSlotDto>? TimeGridSlots { get; set; }
}

public class TimeGridSlotDto
{
    public int Day { get; set; } = default!;
    public int UnitOfDay { get; set; } = default!;
    public int Start { get; set; } = default!;
    public int End { get; set; } = default!;
    public string? Name { get; set; }
}

public class PeriodDto
{
    public long Id { get; set; } = default!;
    public LocalDateTime Modified { get; set; }
    public LocalDateTime Start { get; set; }
    public LocalDateTime End { get; set; }
    public PeriodTypeEnum Type { get; set; } = default!;
    public PeriodStatusEnum Status { get; set; } = default!;
    public long? RelatedPeriod { get; set; }
    public List<PeriodIconEnum>? Icons { get; set; }
    public long? TimeGridId { get; set; }
    public ExamDto? Exam { get; set; }
    public List<PeriodResourceDto>? Departments { get; set; }
    public List<PeriodResourceDto>? Classes { get; set; }
    public List<PeriodResourceDto>? StudentGroups { get; set; }
    public List<PeriodResourceDto>? Students { get; set; }
    public List<PeriodResourceDto>? Subjects { get; set; }
    public List<PeriodResourceDto>? Rooms { get; set; }
    public List<PeriodResourceDto>? Teachers { get; set; }
}

public class ExamDto
{
    public long Id { get; set; } = default!;
    public string Type { get; set; } = null!;
    public string? Name { get; set; }
}

public class PeriodResourceDto : ResourceRefDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ResourceStatusEnum Status { get; set; } = default!;
}

public class ResourceRefDto
{
    public string Discriminator { get; set; } = null!;
    public long Id { get; set; } = default!;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PeriodTypeEnum
{
    NORMAL_TEACHING_PERIOD,
    ADDITIONAL_PERIOD,
    STAND_BY_PERIOD,
    OFFICE_HOUR,
    EXAM,
    BREAK_SUPERVISION,
    EVENT,
    MEETING
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PeriodStatusEnum
{
    REGULAR,
    CANCELLED,
    ADDITIONAL,
    CHANGED
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceStatusEnum
{
    REGULAR,
    ADDED,
    REMOVED
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PeriodIconEnum
{
    ONLINE
}