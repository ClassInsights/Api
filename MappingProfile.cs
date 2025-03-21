using Api.Models.Database;
using Api.Models.Dto;
using AutoMapper;

namespace Api;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<MasterDataObjectDto, Room>().ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.Id));
        CreateMap<MasterDataObjectDto, Subject>().ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.Id));
        CreateMap<MasterDataObjectDto, Class>().ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id));
        CreateMap<ApiDto.RoomDto, Room>();
        CreateMap<ApiDto.ClassDto, Class>().ReverseMap();
        CreateMap<ApiDto.LessonDto, Lesson>().ReverseMap();
        CreateMap<ApiDto.SubjectDto, Subject>().ReverseMap();
        CreateMap<ApiDto.ComputerDto, Computer>().ReverseMap();
        CreateMap<Room, ApiDto.RoomDto>()
            .ConstructUsing(x => new ApiDto.RoomDto(x.RoomId, x.DisplayName!, x.Regex!, x.Enabled, null));
    }
}