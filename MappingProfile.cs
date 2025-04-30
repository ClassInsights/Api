using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;

namespace Api;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UntisMasterDataObject, Room>().ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.Id));
        CreateMap<UntisMasterDataObject, Subject>()
            .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.Id));
        CreateMap<UntisMasterDataObject, Class>().ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id));
        CreateMap<RoomDto, Room>();
        CreateMap<ClassDto, Class>().ReverseMap();
        CreateMap<LessonDto, Lesson>().ReverseMap();
        CreateMap<SubjectDto, Subject>().ReverseMap();
        CreateMap<ComputerDto, Computer>().ReverseMap();
        CreateMap<Room, RoomDto>()
            .ConstructUsing(x => new RoomDto(x.RoomId, x.Enabled, x.DisplayName, x.Regex, null));
    }
}