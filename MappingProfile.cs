using Api.Models;
using AutoMapper;

namespace Api;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApiModels.Class, TabClass>().ReverseMap();
        CreateMap<ApiModels.Lesson, TabLesson>().ReverseMap();
        CreateMap<ApiModels.Computer, TabComputer>().ReverseMap();
        CreateMap<ApiModels.Room, TabRoom>().ReverseMap();
    }
}