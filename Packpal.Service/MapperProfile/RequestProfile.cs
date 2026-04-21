using AutoMapper;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.MapperProfile
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {

            CreateMap<Request, ViewRequestModel>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : ""))
                .ReverseMap();
            CreateMap<Request, CreateRequestModel>().ReverseMap();
        }
    }
}
