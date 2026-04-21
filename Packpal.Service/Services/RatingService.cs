using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Packpal.BLL.Interface;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Services;

public class RatingService : IRatingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public RatingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /*public async Task<Guid> CreateRatingAsync(CreateRatingModel model)
    {
        if(model == null)
        {
            return Guid.Empty;
        }
        var rating = _mapper.Map<Rating>(model);
        await _unitOfWork.GetRepository<Rating>().InsertAsync(rating);
        await _unitOfWork.SaveAsync();
        return rating.Id;
    }

    public async Task<bool> DeleteRatingAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }
        var rating = await _unitOfWork.GetRepository<Rating>().GetByIdAsync(id);
        if (rating == null)
        {
            return false;
        }
        await _unitOfWork.GetRepository<Rating>().DeleteAsync(rating);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<IEnumerable<ViewRatingModel>> GetAllRatingsAsync(int page, int pageSize)
    {
        var ratings = await _unitOfWork.GetRepository<Rating>()
            .Entities
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return _mapper.Map<IEnumerable<ViewRatingModel>>(ratings);
    }

    public async Task<ViewRatingModel> GetRatingByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }
        var rating = await _unitOfWork.GetRepository<Rating>().GetByIdAsync(id);
        return rating != null ? _mapper.Map<ViewRatingModel>(rating) : null;
    }

    public async Task<IEnumerable<ViewRatingModel>> GetRatingsByRenterIdAsync(Guid renterId, int page, int pageSize)
    {
        var ratings = await _unitOfWork.GetRepository<Rating>()
            .Entities
            .Where(r => r.RenterId == renterId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ViewRatingModel>>(ratings);
    }

    public async Task<IEnumerable<ViewRatingModel>> GetRatingsByStorageIdAsync(Guid storageId, int page, int pageSize)
    {
        var ratings = await _unitOfWork.GetRepository<Rating>()
            .Entities
            .Where(r => r.StorageId == storageId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ViewRatingModel>>(ratings);
    }

    public async Task<Guid> UpdateRatingAsync(UpdateRatingModel model)
    {
        if (model == null || model.Id == Guid.Empty)
        {
            return Guid.Empty;
        }
        var rating = await _unitOfWork.GetRepository<Rating>().GetByIdAsync(model.Id);
        if (rating == null)
        {
            return Guid.Empty;
        }
        _mapper.Map(model, rating);
        await _unitOfWork.GetRepository<Rating>().UpdateAsync(rating);
        await _unitOfWork.SaveAsync();
        return rating.Id;
    }*/

    public async Task<Guid> CreateRatingAsync(CreateRatingModel model)
    {
        try
        {
            _unitOfWork.BeginTransaction();
			if (model == null)
			{
				return Guid.Empty;
			}
			var rating = _mapper.Map<Rating>(model);
			await _unitOfWork.GetRepository<Rating>().InsertAsync(rating);
			await _unitOfWork.SaveAsync();
			_unitOfWork.CommitTransaction();
			return rating.Id;
		}
		catch (Exception e)
		{
			_unitOfWork.RollBack();
			throw new Exception(e.Message);
		}
	}

    public async Task<bool> DeleteRatingAsync(Guid id)
    {
        try
        {
			_unitOfWork.BeginTransaction();
			if (id == Guid.Empty)
			{
				return false;
			}
			var rating = await _unitOfWork.GetRepository<Rating>().GetByIdAsync(id);
			if (rating == null)
			{
				return false;
			}
			await _unitOfWork.GetRepository<Rating>().DeleteAsync(rating);
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

    public async Task<PagingModel<ViewRatingModel>> GetAllRatingsAsync(RatingQuery query)
    {
        try
        {
			var ratings = await _unitOfWork.GetRepository<Rating>().GetAllAsync();

			var ratingView = ratings.Select(
				r => _mapper.Map<ViewRatingModel>(r));

			var pagedData = PagingExtension.ToPagingModel(ratingView, query.PageIndex, query.PageSize);

			return pagedData;
		}
		catch (Exception e)
		{			
			Console.WriteLine(e);
			throw new Exception(e.Message);
		}
	}

    public async Task<ViewRatingModel> GetRatingByIdAsync(Guid id)
    {
        try
        {
			if (id == Guid.Empty)
			{
				return null;
			}
			var rating = await _unitOfWork.GetRepository<Rating>().GetByIdAsync(id);
			if (rating == null)
			{
				return null;
			}
			// Map the Rating entity to ViewRatingModel
			var viewModel = _mapper.Map<ViewRatingModel>(rating);

			return viewModel;
		}
		catch (Exception e)
		{

			throw new Exception(e.Message);
		}
	}

    public async Task<PagingModel<ViewRatingModel>> GetRatingsByRenterIdAsync(Guid renterId, RatingQuery query)
    {
		try
		{
			if (renterId == Guid.Empty)
			{
				return null;
			}

			var ratings = await _unitOfWork.GetRepository<Rating>().GetAllAsync();
			if (ratings == null || !ratings.Any())
			{
				return new PagingModel<ViewRatingModel>
				{
					PageIndex = query.PageIndex,
					PageSize = query.PageSize,
					TotalCount = 0,
					TotalPages = 0,
					Data = new List<ViewRatingModel>()
				};
			}
			ratings = ratings.Where(r => r.RenterId == renterId);

			var ratingView = ratings.Select(r => _mapper.Map<ViewRatingModel>(r));

			var pagedData = PagingExtension.ToPagingModel(ratingView, query.PageIndex, query.PageSize);

			return pagedData;
		}
		catch (Exception e)
		{
	
			throw new Exception(e.Message);
		}
	}

    public async Task<PagingModel<ViewRatingModel>> GetRatingsByStorageIdAsync(Guid storageId, RatingQuery query)
    {
		try
		{
			var ratings = await _unitOfWork.GetRepository<Rating>().GetAllAsync();
			if (ratings == null || !ratings.Any())
			{
				return new PagingModel<ViewRatingModel>
				{
					PageIndex = query.PageIndex,
					PageSize = query.PageSize,
					TotalCount = 0,
					TotalPages = 0,
					Data = new List<ViewRatingModel>()
				};
			}
			ratings = ratings.Where(r => r.StorageId == storageId);

			var ratingView = ratings.Select(r => _mapper.Map<ViewRatingModel>(r));

			var pagedData = PagingExtension.ToPagingModel(ratingView, query.PageIndex, query.PageSize);

			return pagedData;
		}
		catch (Exception e)
		{			
			throw new Exception(e.Message);
		}
	}

    public async Task<Guid> UpdateRatingAsync(UpdateRatingModel model)
    {
		try
		{
			_unitOfWork.BeginTransaction();

			if (model == null || model.Id == Guid.Empty)
			{
				return Guid.Empty;
			}
			var rating = await _unitOfWork.GetRepository<Rating>().GetByIdAsync(model.Id);
			if (rating == null)
			{
				return Guid.Empty;
			}
			// Map the model data TO the existing tracked entity
			_mapper.Map(model, rating);
			await _unitOfWork.GetRepository<Rating>().UpdateAsync(rating);
			await _unitOfWork.SaveAsync();

			_unitOfWork.CommitTransaction();

			return rating.Id;
		}
		catch (Exception e)
		{
			_unitOfWork.RollBack();
			throw new Exception(e.Message);
		}
	}
}
