using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dto;

public record ClassDto(
    [property: Description("Id of the class")]
    long? ClassId,
    [MaxLength(20)]
    [property: Description("Name of the class")]
    string? DisplayName,
    [MaxLength(50)]
    [property: Description("Id of the azure group which is associated with the class")]
    string? AzureGroupId
);