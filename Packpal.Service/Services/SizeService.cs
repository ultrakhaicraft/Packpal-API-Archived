using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Packpal.BLL.Interface;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Services;

public class SizeService : ISizeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SizeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> CreateSizeAsync(CreateSizeModel model)
    {
        try
        {
            _unitOfWork.BeginTransaction();
            if (model == null)
            {
                return Guid.Empty;
            }

            var size = _mapper.Map<Size>(model);

            await _unitOfWork.GetRepository<Size>().InsertAsync(size);
            await _unitOfWork.SaveAsync();
            _unitOfWork.CommitTransaction();
            return size.Id;
        }
        catch (Exception e)
        {
            _unitOfWork.RollBack();
            throw new Exception(e.Message);
        }
    }
    public async Task<bool> DeleteSizeAsync(Guid id)
    {
        try
        {
            _unitOfWork.BeginTransaction();
			var size = await _unitOfWork.GetRepository<Size>().GetByIdAsync(id);
				if (size == null)
				{
					return false;
				}
				await _unitOfWork.GetRepository<Size>().DeleteAsync(size);
				await _unitOfWork.SaveAsync();
            _unitOfWork.CommitTransaction();

				return true;
			}
        catch (Exception e)
        {
            _unitOfWork.RollBack();
            throw new Exception(e.Message);
        }
    }
    public async Task<PagingModel<ViewSizeModel>> GetAllSizesAsync(SizeQuery query)
    {
        try
        {
            /*var sizes = await _unitOfWork.GetRepository<Size>().GetAllAsync();
			if (sizes == null || !sizes.Any())
			{
				return new PagingModel<ViewSizeModel>
				{
					Data = new List<ViewSizeModel>(),
					TotalCount = 0
				};
			}

			 var sizeView= sizes.Select(size => _mapper.Map<ViewSizeModel>(sizes));

			var pagedData = PagingExtension.ToPagingModel(sizeView, query.PageIndex, query.PageSize);*/
            var sizes = await _unitOfWork.GetRepository<Size>()
                .Entities
                .ToListAsync();

            if (sizes == null || !sizes.Any())
            {
                return new PagingModel<ViewSizeModel>
                {
                    Data = new List<ViewSizeModel>(),
                    TotalCount = 0
                };
            }

            var sizeViews = _mapper.Map<IEnumerable<ViewSizeModel>>(sizes);

            var pagedData = PagingExtension.ToPagingModel(sizeViews, query.PageIndex, query.PageSize);


            return pagedData;
		}
        catch (Exception e)
        {

			throw new Exception(e.Message);
		}
    }

    public async Task<ViewSizeModel> GetSizeByIdAsync(Guid id)
    {
        try
        {
			var size = await _unitOfWork.GetRepository<Size>().GetByIdAsync(id);
            if (size == null)
            {
				return null;
			}
            var sizeView = _mapper.Map<ViewSizeModel>(size);
            sizeView.OrderCount = size.OrderDetails.Count;

            return sizeView;
        }
        catch (Exception e)
        {
			throw new Exception(e.Message);
		}
    }

    public async Task<Guid> UpdateSizeAsync(UpdateSizeModel model)
    {
        try
        {
            _unitOfWork.BeginTransaction();
            if (model == null)
            {
                return Guid.Empty;
            }
            var size = await _unitOfWork.GetRepository<Size>().GetByIdAsync(model.Id);
            if (size == null)
            {
                return Guid.Empty;
            }
            _mapper.Map(model, size);
            await _unitOfWork.GetRepository<Size>().UpdateAsync(size);
            await _unitOfWork.SaveAsync();
            _unitOfWork.CommitTransaction();
			return size.Id;
		}

        catch (Exception e)
        {
            _unitOfWork.RollBack();
			throw new Exception(e.Message);
		}
    }
}
