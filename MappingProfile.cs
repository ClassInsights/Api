using Api.Models;
using AutoMapper;

namespace Api;

/// <inheritdoc />
public class MappingProfile : Profile
{
    /// <inheritdoc />
    public MappingProfile()
    {
        CreateMap<ApiModels.Room, Room>();
        CreateMap<ApiModels.Class, Class>().ReverseMap();
        CreateMap<ApiModels.Lesson, Lesson>().ReverseMap();
        CreateMap<ApiModels.Subject, Subject>().ReverseMap();
        CreateMap<ApiModels.Computer, Computer>().ReverseMap();
        CreateMap<Room, ApiModels.Room>()
            .ConstructUsing(x => new ApiModels.Room(x.RoomId, x.Name!, x.LongName!, null));
    }
}