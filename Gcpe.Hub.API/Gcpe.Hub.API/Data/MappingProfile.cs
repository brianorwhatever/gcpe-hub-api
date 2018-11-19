using AutoMapper;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.API.ViewModels;

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
