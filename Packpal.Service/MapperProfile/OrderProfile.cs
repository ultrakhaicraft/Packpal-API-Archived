using AutoMapper;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.MapperProfile
{
	public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, ExtendedOrderViewModel>()
                .ForMember(dest => dest.RenterName, opt => opt.MapFrom(src => src.Renter != null && src.Renter.User != null ? src.Renter.User.Username : string.Empty))
                .ForMember(dest => dest.RenterEmail, opt => opt.MapFrom(src => src.Renter != null && src.Renter.User != null ? src.Renter.User.Email : string.Empty))
                .ForMember(dest => dest.RenterUsername, opt => opt.MapFrom(src => src.Renter != null && src.Renter.User != null ? src.Renter.User.Username : string.Empty))
                .ForMember(dest => dest.StorageAddress, opt => opt.MapFrom(src => src.Storage != null ? src.Storage.Address : string.Empty))
                .ReverseMap();
            CreateMap<Order, UpdateOrderModel>().ReverseMap();
            CreateMap<Order, CreateOrderModel>().ReverseMap();
            CreateMap<Order, ViewSummaryOrderModel>().ReverseMap();
        }
    }
}
