using AutoMapper;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.MapperProfile
{
	public class RatingProfile : Profile
    {
        public RatingProfile(){
            CreateMap<Rating, ViewRatingModel>().ReverseMap();
            CreateMap<Rating, CreateRatingModel>().ReverseMap();
            CreateMap<Rating, UpdateRatingModel>().ReverseMap();
        }
    }
}
