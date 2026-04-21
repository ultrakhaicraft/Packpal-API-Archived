using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Interface
{
	public interface IOrderDetailService
    {
        Task<string> CreateOrderDetailAsync(List<CreateOrderDetailModel> models, Guid orderId);
		Task<Guid> UpdateOrderDetailAsync(UpdateOrderDetailModel model);
        Task<bool> DeleteOrderDetailAsync(Guid orderDetailId);
        Task<ViewOrderDetailModel> GetOrderDetailByIdAsync(Guid orderDetailId);
        Task<PagingModel<ViewOrderDetailModel>> GetAllOrderDetailsByOrderIdAsync(Guid orderId, int page = 1, int pageSize = 5);
    }
}
