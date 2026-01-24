using AutoMapper;
using LanguageLearnNETWebAPI.Models;

namespace LanguageLearnNETWebAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<WordEntity, Word>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RowKey));

            CreateMap<Word, WordEntity>()
            .ForMember(dest => dest.PartitionKey, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RowKey, opt => opt.MapFrom(src => src.Id));

            CreateMap<WordBase, Word>();
        }
    }
}
