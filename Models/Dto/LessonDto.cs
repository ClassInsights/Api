using System.ComponentModel;
using NodaTime;

namespace Api.Models.Dto;

public record LessonDto(
    [property: Description("Id of lesson")]
    long LessonId,
    [property: Description("Id of the room the lesson takes place in")]
    long RoomId,
    [property: Description("Id of the subject the lesson is about")]
    long SubjectId,
    [property: Description("Id of the class which has the lesson")]
    long ClassId,
    [property: Description("Time when the lesson starts")]
    Instant? Start,
    [property: Description("Time when the lesson ends")]
    Instant? End
);