using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Interface;

public interface ISizeService
{
    Task<PagingModel<ViewSizeModel>> GetAllSizesAsync(SizeQuery query);
    Task<ViewSizeModel> GetSizeByIdAsync(Guid id);
    Task<Guid> CreateSizeAsync(CreateSizeModel model);
    Task<Guid> UpdateSizeAsync(UpdateSizeModel model);
    Task<bool> DeleteSizeAsync(Guid id);
}