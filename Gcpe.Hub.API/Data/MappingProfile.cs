using AutoMapper;
using Gcpe.Hub.API.ViewModels;
using Gcpe.Hub.Data.Entity;

namespace Gcpe.Hub.API.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<NewsRelease, NewsReleaseViewModel>()
                .ReverseMap();

            CreateMap<NewsReleaseLog, NewsReleaseLogViewModel>()
                .ReverseMap();
        }
    }
}
