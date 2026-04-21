using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Interface;

public interface IOrderService
{
	Task<PagingModel<ViewSummaryOrderModel>> GetAllOrderFromAStorageAsync(OrderQuery request, Guid storageId);
	Task<PagingModel<ViewSummaryOrderModel>> GetAllOrderFromAUserAsync(OrderQuery request, Guid userId);
	Task<PagingModel<ViewSummaryOrderModel>> GetAllOrderFromAKeeperAsync(OrderQuery request, Guid keeperId);
	Task<ExtendedOrderViewModel> GetOrderDetailByIdAsync(Guid orderId);
    Task<PagingModel<ViewSummaryOrderModel>> GetAllOrderAsync(OrderQuery request);
	Task<Guid> CreateOrderAsync(CreateOrderModel model);
	Task<Guid> UpdateOrderAsync(UpdateOrderModel model);
	Task<Guid> PatchUpdateOrderAsync(PatchOrderModel model);
	Task<bool> DeleteOrderAsync(Guid orderId);
	Task<bool> UpdateOrderStatusAsync(Guid orderId, string status);
	Task<bool> UpdateIsPaidAsync(Guid orderId);
	Task<bool> StartKeepTimeAsync(Guid orderId);
	Task<double> CalculateFinalAmountAsync(Guid orderId);
	Task<bool> UpdateOrderCertificationAsync(Guid orderId, string[] imageUrls);
	Task<double> GetTotalAmountAsync(Guid orderId);
	Task<OrderCountdownModel?> GetOrderCountdownAsync(Guid orderId);
	Task<List<OrderCountdownModel>> GetMultipleOrderCountdownAsync(List<Guid> orderIds);
}
