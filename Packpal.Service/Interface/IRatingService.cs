using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Interface;

public interface IRatingService
{
    Task<PagingModel<ViewRatingModel>> GetAllRatingsAsync(RatingQuery query);
    Task<PagingModel<ViewRatingModel>> GetRatingsByStorageIdAsync(Guid storageId, RatingQuery query);
    Task<PagingModel<ViewRatingModel>> GetRatingsByRenterIdAsync(Guid renterId, RatingQuery query);
    Task<ViewRatingModel> GetRatingByIdAsync(Guid id);
    Task<Guid> CreateRatingAsync(CreateRatingModel model);
    Task<Guid> UpdateRatingAsync(UpdateRatingModel model);
    Task<bool> DeleteRatingAsync(Guid id);
}