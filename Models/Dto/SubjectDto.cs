using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dto;

public record SubjectDto(
    [property: Description("Id of the subject")]
    long SubjectId,
    [property: Description("Name of the subject")]
    [MaxLength(100)]
    string DisplayName
);