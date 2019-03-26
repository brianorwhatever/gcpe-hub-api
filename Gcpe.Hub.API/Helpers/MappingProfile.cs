using System;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.Data.Entity;

namespace Gcpe.Hub.API.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Activity, Models.Activity>()
                .ForMember(dest => dest.MinistriesSharedWith, opt => opt.MapFrom(src => src.ActivitySharedWith.Select(sw => sw.Ministry.Key)))
                .ForMember(dest => dest.ContactMinistryKey, opt => opt.MapFrom(src => src.ContactMinistry.Key))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.ActivityCategories.Select(ac => ac.Category.Name)));
            // use db.Entry(dbActivity).CurrentValues.SetValues() instead of ReverseMap

            CreateMap<NewsReleaseDocumentLanguage, Models.Post.Document>();

            CreateMap<NewsRelease, Models.Post>()
                .ForMember(dest => dest.Kind, opt => opt.MapFrom(src => src.ReleaseType));

            CreateMap<NewsReleaseLog, Models.PostLog>() // use db.Entry(dbPostLog).CurrentValues.SetValues() instead of ReverseMap
                .ForMember(dest => dest.PostKey, opt => opt.MapFrom(src => src.Release.Key));

            CreateMap<Message, Models.Message>();
            // use db.Entry(dbMessage).CurrentValues.SetValues() instead of ReverseMap

            CreateMap<SocialMediaPost, Models.SocialMediaPost>();
        }
    }
}
