using Packpal.DAL.Enum;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Interface
{
    public interface IRequestService
    {
        Task<ViewRequestModel> CreateAsync(CreateRequestModel model);
        Task<PagingModel<ViewRequestModel>> GetAllAsync(RequestQuery query);
        Task<ViewRequestModel?> GetByIdAsync(Guid id);
        Task<ViewRequestModel?> UpdateStatusAsync(Guid requestId, Guid userId,RequestStatusEnum status);
        Task<IEnumerable<ViewRequestModel>> GetByUserIdAsync(Guid userId);
    }
}
