using AutoMapper;
using SmartServices.models.Dto;
using SmartServices.models.Entity;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartServices
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<models.Entity.Job, JobDto>().ReverseMap();
            CreateMap<models.Entity.Job, CreateJobDto>().ReverseMap();

            CreateMap<KeyValuePair<string, string>, models.Dto.JobDescriptionDto>()
               .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));
        }
    }
}


