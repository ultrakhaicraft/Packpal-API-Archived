using AutoMapper;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.MapperProfile
{
	public class OrderDetailProfile : Profile
    {
        public OrderDetailProfile()
        {
            CreateMap<OrderDetail, ViewOrderDetailModel>().ReverseMap();
            CreateMap<OrderDetail, UpdateOrderDetailModel>().ReverseMap();
            CreateMap<OrderDetail, CreateOrderDetailModel>().ReverseMap();
        }
    }
}
