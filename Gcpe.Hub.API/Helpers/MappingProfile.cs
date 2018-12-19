using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gcpe.Hub.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Gcpe.Hub.API.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Activity, Models.Activity>()
                .ForMember(dest => dest.MinistriesSharedWith, opt => opt.MapFrom(src => src.ActivitySharedWith.Select(sw => sw.Ministry.Key)))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.ActivityCategories.Select(ac => ac.Category.Name)));
            // use db.Entry(post).CurrentValues.SetValues() instead of ReverseMap

            CreateMap<NewsReleaseDocumentLanguage, Models.Post.Document>();

            CreateMap<NewsRelease, Models.Post>()
                .ForMember(dest => dest.Kind, opt => opt.MapFrom(src => src.ReleaseType));
            //.ForMember(dest => dest.NewsReleaseLanguage, opt => opt.MapFrom(src => src.Summary))
            //.ForMember(dest => dest.NewsReleaseMinistry, opt => opt.MapFrom(src => src.MinistriesSharedWith));

            CreateMap<NewsReleaseLog, Models.PostLog>() // use db.Entry(post).CurrentValues.SetValues() instead of ReverseMap
                .ForMember(dest => dest.PostKey, opt => opt.MapFrom(src => src.Release.Key));

            CreateMap<Message, Models.Message>()
                .ReverseMap();

            CreateMap<SocialMediaPost, Models.SocialMediaPost>()
                .ReverseMap();
        }
    }
}
