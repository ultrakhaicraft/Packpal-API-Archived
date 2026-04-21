using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Packpal.BLL.Interface;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.Enum;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;


namespace Packpal.BLL.Services;

public class StorageService : IStorageService
{
	private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StorageService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(CreateStorageModel model)
    {
        try
        {
			_unitOfWork.BeginTransaction();
			if (model == null)
			{
				return Guid.Empty;
			}
			Storage storage = _mapper.Map<Storage>(model);

			await _unitOfWork.GetRepository<Storage>().InsertAsync(storage);
			await _unitOfWork.SaveAsync();
			_unitOfWork.CommitTransaction();

			return storage.Id;
		}
        catch (Exception e)
        {
			_unitOfWork.RollBack();
		
			throw new Exception(e.Message);
		}
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
			_unitOfWork.BeginTransaction();
            var storage = await _unitOfWork.GetRepository<Storage>()
                .Entities
                .Include(s => s.Orders)
                .Where(s => Guid.Equals(s.Id, id))
                .FirstOrDefaultAsync();
                
			if (storage == null)
				return false;
                
            // Check if storage has active orders (PENDING, CONFIRMED, IN_STORAGE)
            var activeOrders = storage.Orders?.Where(o => 
                o.Status == "PENDING" || 
                o.Status == "CONFIRMED" || 
                o.Status == "IN_STORAGE").ToList();
                
            if (activeOrders != null && activeOrders.Any())
            {
                Console.WriteLine($"❌ [StorageService] Cannot delete storage {id} - has {activeOrders.Count} active orders");
                return false;
            }
                
            storage.Status = StorageStatusEnum.DELETED.ToString();
            _unitOfWork.GetRepository<Storage>().Update(storage);
			await _unitOfWork.SaveAsync();
			_unitOfWork.CommitTransaction();
			
			Console.WriteLine($"✅ [StorageService] Storage {id} marked as DELETED successfully");
			return true;
		}
        catch (Exception e)
        {
			_unitOfWork.RollBack();
			
            throw new Exception(e.Message);
        }
    }

    public async Task<PagingModel<ViewStorageModel>> GetAllAsync(StorageQuery query)
    {
		try
		{
            var storages = await _unitOfWork.GetRepository<Storage>()
			.Entities
            .Where(s => s.Status != StorageStatusEnum.DELETED.ToString()) // Exclude deleted storages
            .Include(s => s.Keeper)
				.ThenInclude(k => k!.User)
			.Include(s => s.Orders)
			.Include(s => s.Ratings)
			.ToListAsync();
				
			if (storages == null || !storages.Any())
				return new PagingModel<ViewStorageModel>
				{
					Data = new List<ViewStorageModel>(),
					TotalCount = 0
				};

			var filteredStorages = storages.AsQueryable();

			if (query.Status.HasValue)
			{
				var statusString = query.Status.Value.ToString();
				filteredStorages = filteredStorages.Where(s => s.Status == statusString);
			}

			//Filter based on Address
			if (!string.IsNullOrEmpty(query.Address))
			{
				filteredStorages = filteredStorages.Where(storage => storage.Address.Contains(query.Address));
			}
			
			var storageList = filteredStorages.ToList();
			
            var storageViews = storageList.Select(s => new ViewStorageModel
			{
				Id = s.Id,
				Status = s.Status.ToString(),
				Description = s.Description,
				Address = s.Address,
				KeeperId = s.KeeperId,
				KeeperName = s.Keeper?.User?.Username ?? "Unknown Keeper",
				KeeperPhoneNumber = s.Keeper?.User?.PhoneNumber ?? "",
				Latitude = s.Latitude,
				Longitude = s.Longitude,
				PendingOrdersCount = s.Orders?.Count(o => o.Status == "PENDING") ?? 0,
				AverageRating = s.Ratings != null && s.Ratings.Any() ? s.Ratings.Average(r => r.Star) : 0.0,
				RatingCount = s.Ratings?.Count ?? 0

			}).ToList();

			var pagedData = PagingExtension.ToPagingModel(storageViews, query.PageIndex, query.PageSize);


			return pagedData;

		}
		catch (Exception e)
		{
			
			throw new Exception(e.Message);
		}
	}

    public async Task<ViewStorageModel?> GetByIdAsync(Guid id)
    {
		try
		{
			var storage = await _unitOfWork.GetRepository<Storage>()
				.Entities
                .Where(s => s.Status != StorageStatusEnum.DELETED.ToString()) // Exclude deleted storages
                .Include(s => s.Keeper)
					.ThenInclude(k => k!.User)
				.Include(s => s.Orders) // Include orders to count pending ones
				.Include(s => s.Ratings) // Include ratings for calculation
				.FirstOrDefaultAsync(s => s.Id == id);

			if (storage == null)
				return null;

			var viewModel = new ViewStorageModel
			{
				Id = storage.Id,
				Status = storage.Status.ToString(),
				Description = storage.Description,
				Address = storage.Address,
				KeeperId = storage.KeeperId,
				KeeperName = storage.Keeper?.User?.Username ?? "Unknown Keeper",
				KeeperPhoneNumber = storage.Keeper?.User?.PhoneNumber ?? "",
				Latitude = storage.Latitude,
				Longitude = storage.Longitude,
				PendingOrdersCount = storage.Orders?.Count(o => o.Status == "PENDING") ?? 0,
				AverageRating = storage.Ratings != null && storage.Ratings.Any() ? storage.Ratings.Average(r => r.Star) : 0.0,
				RatingCount = storage.Ratings?.Count ?? 0
			};

			return viewModel;
		}
		catch (Exception e)
		{
			
			throw new Exception(e.Message);
		}
	}

    public async Task<List<ViewStorageModel>> GetByKeeperIdAsync(Guid keeperId)
    {
        try
        {
            var storages = await _unitOfWork.GetRepository<Storage>()
                .Entities
                .Where(s => s.Status != StorageStatusEnum.DELETED.ToString()) // Exclude deleted storages
                .Include(s => s.Keeper)
                    .ThenInclude(k => k!.User)
                .Include(s => s.Orders) // Include orders to count pending ones
                .Include(s => s.Ratings) // Include ratings for calculation
                .Where(s => s.KeeperId == keeperId)
                .ToListAsync();

            if (storages == null || !storages.Any())
                return new List<ViewStorageModel>();

            var viewModels = storages.Select(s => new ViewStorageModel
            {
                Id = s.Id,
                Status = s.Status.ToString(),
                Description = s.Description,
                Address = s.Address,
                KeeperId = s.KeeperId,
                KeeperName = s.Keeper?.User?.Username ?? "Unknown Keeper",
                KeeperPhoneNumber = s.Keeper?.User?.PhoneNumber ?? "",
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                PendingOrdersCount = s.Orders?.Count(o => o.Status == "PENDING") ?? 0,
                AverageRating = s.Ratings != null && s.Ratings.Any() ? s.Ratings.Average(r => r.Star) : 0.0,
                RatingCount = s.Ratings?.Count ?? 0
            }).ToList();

            return viewModels;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<bool> UpdateAsync(UpdateStorageModel model)
    {
		try
		{
			_unitOfWork.BeginTransaction();
			var storage = await _unitOfWork.GetRepository<Storage>()
                .Entities
                .Where(storage => storage.Status != StorageStatusEnum.DELETED.ToString() 
                && Guid.Equals(storage.Id, model.Id))
                .FirstOrDefaultAsync();

            if (storage == null)
				return false;

			storage = _mapper.Map(model, storage);
			_unitOfWork.GetRepository<Storage>().Update(storage);
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

    public Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km

        double dLat = DegreesToRadians(lat2 - lat1);
        double dLon = DegreesToRadians(lon2 - lon1);

        double a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = R * c;
        
        return Task.FromResult(distance);
    }
    private static double DegreesToRadians(double deg)
    {
        return deg * (Math.PI / 180);
    }

    public async Task<PagingModel<ViewStorageModel>> GetAllByKeeperId(StorageQuery query, Guid keeperId)
    {
        try
        {
            var storages = await _unitOfWork.GetRepository<Storage>()
                .Entities
                .Where(s => s.KeeperId == keeperId 
                    && s.Status != StorageStatusEnum.DELETED.ToString())
                .Include(s => s.Keeper)
                    .ThenInclude(k => k!.User)
                .Include(s => s.Orders)
                .Include(s => s.Ratings)
                .ToListAsync();

            if (storages == null || !storages.Any())
                return new PagingModel<ViewStorageModel>
                {
                    Data = new List<ViewStorageModel>(),
                    TotalCount = 0
                };

            // Apply status filter
            if (query.Status.HasValue)
            {
                var statusString = query.Status.Value.ToString();
                storages = storages.Where(s => s.Status == statusString).ToList();
            }

            // Apply address filter
            if (!string.IsNullOrEmpty(query.Address))
            {
                storages = storages.Where(storage => storage.Address.Contains(query.Address)).ToList();
            }

            var storageViews = storages.Select(s => new ViewStorageModel
            {
                Id = s.Id,
                Status = s.Status.ToString(),
                Description = s.Description,
                Address = s.Address,
                KeeperId = s.KeeperId,
                KeeperName = s.Keeper?.User?.Username ?? "Unknown Keeper",
                KeeperPhoneNumber = s.Keeper?.User?.PhoneNumber ?? "",
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                PendingOrdersCount = s.Orders?.Count(o => o.Status == "PENDING") ?? 0,
                AverageRating = s.Ratings != null && s.Ratings.Any() ? s.Ratings.Average(r => r.Star) : 0.0,
                RatingCount = s.Ratings?.Count ?? 0
            }).ToList();

            var pagedData = PagingExtension.ToPagingModel(storageViews, query.PageIndex, query.PageSize);

            return pagedData;

        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<int> GetTotalPendingOrdersByKeeperIdAsync(Guid keeperId)
    {
        try
        {
            var totalPendingOrders = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Include(o => o.Storage)
                .Where(o => o.Storage!.KeeperId == keeperId && o.Status == "PENDING")
                .CountAsync();

            return totalPendingOrders;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
