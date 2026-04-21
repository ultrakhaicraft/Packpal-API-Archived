using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Interface
{
	public interface IStorageService
    {
        Task<ViewStorageModel?> GetByIdAsync(Guid id);
        Task<PagingModel<ViewStorageModel>> GetAllAsync(StorageQuery query);
        Task<List<ViewStorageModel>> GetByKeeperIdAsync(Guid keeperId);
        Task<Guid> CreateAsync(CreateStorageModel model);
        Task<bool> UpdateAsync(UpdateStorageModel model);
        Task<bool> DeleteAsync(Guid id);
        Task<PagingModel<ViewStorageModel>> GetAllByKeeperId(StorageQuery query, Guid keeperId);
        Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2);
        Task<int> GetTotalPendingOrdersByKeeperIdAsync(Guid keeperId);
    }
}

