using Api.Models;
using AutoMapper;

namespace Api;

/// <inheritdoc />
public class MappingProfile : Profile
{
    /// <inheritdoc />
    public MappingProfile()
    {
        CreateMap<ApiModels.Room, TabRoom>();
        CreateMap<ApiModels.Class, TabClass>().ReverseMap();
        CreateMap<ApiModels.Lesson, TabLesson>().ReverseMap();
        CreateMap<ApiModels.Subject, TabSubject>().ReverseMap();
        CreateMap<ApiModels.Computer, TabComputer>().ReverseMap();
        CreateMap<TabRoom, ApiModels.Room>()
            .ConstructUsing(x => new ApiModels.Room(x.RoomId, x.Name!, x.LongName!, null));
    }
}