using AutoMapper;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.MapperProfile
{
	public class SizeProfile : Profile
    {
        public SizeProfile()
        {
            CreateMap<Size, ViewSizeModel>().ReverseMap();
            CreateMap<Size, CreateSizeModel>().ReverseMap();
            CreateMap<Size, UpdateSizeModel>().ReverseMap();
        }
    }
}
