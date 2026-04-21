using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Packpal.BLL.Interface;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IStorageService _storageService;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IStorageService storageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
        _storageService = storageService;
    }
    public async Task<Guid> CreateOrderAsync(CreateOrderModel model)
    {
        try
        {
                _unitOfWork.BeginTransaction();
                if (model == null)
                {
                    return Guid.Empty;
                }

                Order order = _mapper.Map<Order>(model);

                await _unitOfWork.GetRepository<Order>().InsertAsync(order);
                await _unitOfWork.SaveAsync();
                _unitOfWork.CommitTransaction();

                // Send notification to keeper about new order
                try
                {
                    // Get storage to find keeper
                    var storage = await _unitOfWork.GetRepository<Storage>()
                        .Entities
                        .Include(s => s.Keeper)
                            .ThenInclude(k => k!.User)
                        .FirstOrDefaultAsync(s => s.Id == order.StorageId);

                    if (storage?.Keeper != null)
                    {
                        var orderData = new
                        {
                            OrderId = order.Id,
                            RenterId = order.RenterId,
                            StorageId = order.StorageId,
                            StorageAddress = storage.Address,
                            PackageDescription = order.PackageDescription,
                            TotalAmount = order.TotalAmount,
                            OrderDate = order.OrderDate,
                            Status = order.Status
                        };

                        // Send notification
                        await _notificationService.NotifyKeeperNewOrderAsync(storage.KeeperId, orderData);

                        // Update pending orders count
                        var pendingCount = await _storageService.GetTotalPendingOrdersByKeeperIdAsync(storage.KeeperId);
                        await _notificationService.UpdateKeeperPendingCountAsync(storage.KeeperId, pendingCount);
                    }
                }
                catch (Exception notifyEx)
                {
                    // Log notification error but don't fail the order creation
                    Console.WriteLine($"Failed to send notification: {notifyEx.Message}");
                }

                return order.Id;
            }
            catch (Exception e)
            {
                _unitOfWork.RollBack();
                throw new Exception(e.Message);
            }
    }
    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        try
        {
            /*
            var order = await _unitOfWork.GetRepository<Order>()
            .Entities
            .Where(o => Guid.Equals(o.Id, orderId))
            .FirstOrDefaultAsync();
             */
            _unitOfWork.BeginTransaction();

            var order = _unitOfWork.GetRepository<Order>().GetById(orderId);

            if (order == null)
                return false;

            _unitOfWork.GetRepository<Order>().Delete(order);
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
    //Only show some information
    public async Task<PagingModel<ViewSummaryOrderModel>> GetAllOrderFromAStorageAsync(OrderQuery request, Guid storageId)
    {
        try
        {
            var orders = await _unitOfWork.GetRepository<Order>().GetAllAsync();
            orders = orders
                .Include(o => o.Renter!)
                    .ThenInclude(r => r.User!)
                .Where(o => o.StorageId == storageId);
            
            if (orders == null || !orders.Any())
            {
                return new PagingModel<ViewSummaryOrderModel>
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    TotalPages = 0,
                    Data = new List<ViewSummaryOrderModel>()
                };
            }

            // Only filter by IsPaid if the parameter is specified
            if (request.IsPaid.HasValue)
            {
                if (request.IsPaid.Value)
                {
                    orders = orders.Where(o => o.IsPaid);
                }
                else
                {
                    orders = orders.Where(o => !o.IsPaid);
                }
            }

            if (request.Status.HasValue)
            {
                orders = orders.Where(o => o.Status == request.Status.Value.ToString());
            }

            if (request.MonthAndYear.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Month == request.MonthAndYear.Value.Month
                && o.OrderDate.Year == request.MonthAndYear.Value.Year);
            }

            var orderView = orders.Select(o => new ViewSummaryOrderModel
			{
				Id = o.Id,
				StorageId = o.StorageId,
				RenterId = o.RenterId,
				Status = o.Status,
				TotalAmount = o.TotalAmount,
				PackageDescription = o.PackageDescription,
				OrderDate = o.OrderDate,
				IsPaid = o.IsPaid,
				Renter = o.Renter != null && o.Renter.User != null ? new RenterSummaryModel
				{
					Id = o.Renter.Id,
					Username = o.Renter.User.Username,
					Name = o.Renter.User.Username,
					Email = o.Renter.User.Email
				} : null
			});

            var pagedData = PagingExtension.ToPagingModel(orderView, request.PageIndex, request.PageSize);

            return pagedData;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public async Task<PagingModel<ViewSummaryOrderModel>> GetAllOrderFromAUserAsync(OrderQuery request, Guid userId)
    {
        try
        {
            Console.WriteLine($"🔍 GetAllOrderFromAUserAsync called with userId: {userId}");
            
            // First, find the Renter associated with this userId
            var renter = await _unitOfWork.GetRepository<Renter>().GetAllAsync();
            var userRenter = renter.FirstOrDefault(r => r.UserId == userId);
            
            Console.WriteLine($"🔍 Found renter for userId {userId}: {userRenter?.Id}");
            
            if (userRenter == null)
            {
                Console.WriteLine($"❌ No renter profile found for userId: {userId}");
                // No renter profile found for this user
                return new PagingModel<ViewSummaryOrderModel>
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    TotalPages = 0,
                    Data = new List<ViewSummaryOrderModel>()
                };
            }
            
            // Now get orders using the renterId
            var orders = await _unitOfWork.GetRepository<Order>().GetAllAsync();
            orders = orders.Where(o => o.RenterId == userRenter.Id);

            if (orders == null || !orders.Any())
            {
                return new PagingModel<ViewSummaryOrderModel>
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    TotalPages = 0,
                    Data = new List<ViewSummaryOrderModel>()
                };
            }

            if (request.IsPaid.HasValue)
            {
                if (request.IsPaid.Value)
                {
                    orders = orders.Where(o => o.IsPaid);
                }
                else
                {
                    orders = orders.Where(o => !o.IsPaid);
                }
            }

            if (request.Status.HasValue)
            {
                orders = orders.Where(o => o.Status == request.Status.Value.ToString());
            }

            if (request.MonthAndYear.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Month == request.MonthAndYear.Value.Month
                && o.OrderDate.Year == request.MonthAndYear.Value.Year);
            }

            var orderView = orders.Select(o => new ViewSummaryOrderModel
			{
				Id = o.Id,
				StorageId = o.StorageId,
				RenterId = o.RenterId,
				Status = o.Status,
				TotalAmount = o.TotalAmount,
				PackageDescription = o.PackageDescription,
				OrderDate = o.OrderDate,
				IsPaid = o.IsPaid,
			});

            var pagedData = PagingExtension.ToPagingModel(orderView, request.PageIndex, request.PageSize);

            return pagedData;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    //Get all orders from a keeper by keeper ID
    public async Task<PagingModel<ViewSummaryOrderModel>> GetAllOrderFromAKeeperAsync(OrderQuery request, Guid keeperId)
    {
        try
        {
            Console.WriteLine($"🔍 GetAllOrderFromAKeeperAsync called with keeperId: {keeperId}");
            
            // First, get all storages belonging to this keeper
            var keeperStorages = await _unitOfWork.GetRepository<Storage>()
                .Entities
                .Where(s => s.KeeperId == keeperId)
                .Select(s => s.Id)
                .ToListAsync();
            
            Console.WriteLine($"🏪 Found {keeperStorages.Count} storages for keeper: {keeperId}");
            
            if (!keeperStorages.Any())
            {
                return new PagingModel<ViewSummaryOrderModel>
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    TotalPages = 0,
                    Data = new List<ViewSummaryOrderModel>()
                };
            }
            
            // Get all orders from keeper's storages
            var orders = await _unitOfWork.GetRepository<Order>().GetAllAsync();
            orders = orders
                .Where(o => keeperStorages.Contains(o.StorageId))
                .Include(o => o.Renter!)
                    .ThenInclude(r => r.User!)
                .Include(o => o.Storage);
            
            Console.WriteLine($"📦 Found {orders.Count()} orders for keeper's storages");

            if (orders == null || !orders.Any())
            {
                return new PagingModel<ViewSummaryOrderModel>
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    TotalPages = 0,
                    Data = new List<ViewSummaryOrderModel>()
                };
            }

            // Apply filters
            if (request.IsPaid.HasValue)
            {
                if (request.IsPaid.Value)
                {
                    orders = orders.Where(o => o.IsPaid);
                }
                else
                {
                    orders = orders.Where(o => !o.IsPaid);
                }
            }

            if (request.Status.HasValue)
            {
                orders = orders.Where(o => o.Status == request.Status.Value.ToString());
            }

            if (request.MonthAndYear.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Month == request.MonthAndYear.Value.Month
                && o.OrderDate.Year == request.MonthAndYear.Value.Year);
            }

            var orderView = orders.Select(o => new ViewSummaryOrderModel
            {
                Id = o.Id,
                StorageId = o.StorageId,
                RenterId = o.RenterId,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                PackageDescription = o.PackageDescription,
                OrderDate = o.OrderDate,
                IsPaid = o.IsPaid,
                Renter = o.Renter != null && o.Renter.User != null ? new RenterSummaryModel
                {
                    Id = o.Renter.Id,
                    Username = o.Renter.User.Username,
                    Name = o.Renter.User.Username,
                    Email = o.Renter.User.Email
                } : null
            });

            var pagedData = PagingExtension.ToPagingModel(orderView, request.PageIndex, request.PageSize);

            return pagedData;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    //Show all information
    public async Task<ExtendedOrderViewModel> GetOrderDetailByIdAsync(Guid orderId)
    {
        var order = await _unitOfWork.GetRepository<Order>()
            .Entities
            .Where(o => o.Id == orderId)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Size)
            .Include(o => o.Renter)
                .ThenInclude(r => r!.User)
            .Include(o => o.Storage)
            .FirstOrDefaultAsync();

        var extendedViewOrder = _mapper.Map<ExtendedOrderViewModel>(order);
        extendedViewOrder.RenterName = order?.Renter?.User?.Username ?? string.Empty;
        extendedViewOrder.StorageAddress = order?.Storage?.Address ?? string.Empty;
        return extendedViewOrder;
    }
    public async Task<bool> StartKeepTimeAsync(Guid orderId)
    {
        try
        {
            _unitOfWork.BeginTransaction();
            var orders = _unitOfWork.GetRepository<Order>().GetById(orderId);
            if (orders == null)
            {
                return false;
            }

            orders.StartKeepTime = DateTime.UtcNow;
            orders.Status = "IN_STORAGE";
            _unitOfWork.GetRepository<Order>().Update(orders);
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
    public async Task<bool> UpdateIsPaidAsync(Guid orderId)
    {
        try
        {
            _unitOfWork.BeginTransaction();
            var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(orderId);

            if (order == null)
            {
                return false;
            }
            if (order.IsPaid)
            {
                return false; // Already paid
            }
            order.IsPaid = true;
            _unitOfWork.GetRepository<Order>().Update(order);
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
    public async Task<Guid> UpdateOrderAsync(UpdateOrderModel model)
    {
        try
        {
            _unitOfWork.BeginTransaction();
            var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(model.Id);

            if (order == null)
            {
                return Guid.Empty;
            }
            _mapper.Map(model, order);
            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.SaveAsync();
            _unitOfWork.CommitTransaction();

            return model.Id;
        }
        catch (Exception e)
        {
            _unitOfWork.RollBack();
            throw new Exception(e.Message);
        }
    }
    public async Task<Guid> PatchUpdateOrderAsync(PatchOrderModel model)
    {
        try
        {
            _unitOfWork.BeginTransaction();
            var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(model.Id);

            if (order == null)
            {
                return Guid.Empty;
            }
            
            // Only update fields that are not null in the patch model
            if (!string.IsNullOrEmpty(model.Status))
                order.Status = model.Status;
            
            if (model.TotalAmount.HasValue)
                order.TotalAmount = model.TotalAmount.Value;
                
            if (!string.IsNullOrEmpty(model.PackageDescription))
                order.PackageDescription = model.PackageDescription;
                
            if (model.EstimatedDays.HasValue)
                order.EstimatedDays = model.EstimatedDays.Value;
                
            if (model.IsPaid.HasValue)
                order.IsPaid = model.IsPaid.Value;

            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.SaveAsync();
            _unitOfWork.CommitTransaction();

            return model.Id;
        }
        catch (Exception e)
        {
            _unitOfWork.RollBack();
            throw new Exception(e.Message);
        }
    }
    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
    {
       try
            {
                _unitOfWork.BeginTransaction();
                if (string.IsNullOrEmpty(status))
                {
                    return false;
                }
                var order = await _unitOfWork.GetRepository<Order>()
                    .Entities
                    .Include(o => o.Storage)
                        .ThenInclude(s => s!.Keeper)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return false;
                }

                var oldStatus = order.Status;
                order.Status = status;
                _unitOfWork.GetRepository<Order>().Update(order);
                await _unitOfWork.SaveAsync();
                _unitOfWork.CommitTransaction();

                // Send notification if status changed
                if (oldStatus != status && order.Storage?.Keeper != null)
                {
                    try
                    {
                        // Notify keeper about status change
                        await _notificationService.NotifyKeeperOrderStatusChangeAsync(
                            order.Storage.KeeperId, 
                            orderId, 
                            status
                        );

                        // Notify renter about status change
                        var keeperName = order.Storage.Keeper.User?.Username ?? "Storage Keeper";
                        await _notificationService.NotifyRenterOrderStatusChangeAsync(
                            order.RenterId, 
                            orderId, 
                            status, 
                            keeperName
                        );

                        // Update pending count if status changed from/to PENDING
                        if (oldStatus == "PENDING" || status == "PENDING")
                        {
                            var pendingCount = await _storageService.GetTotalPendingOrdersByKeeperIdAsync(order.Storage.KeeperId);
                            await _notificationService.UpdateKeeperPendingCountAsync(order.Storage.KeeperId, pendingCount);
                        }
                    }
                    catch (Exception notifyEx)
                    {
                        Console.WriteLine($"Failed to send status change notification: {notifyEx.Message}");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _unitOfWork.RollBack();
                throw new Exception(e.Message);
            }
        }
    public async Task<double> GetTotalAmountAsync(Guid orderId)
    {
        var order = await GetOrderDetailByIdAsync(orderId);
        if(order.StartKeepTime == null)
        {
            return -1.0;
        }
        foreach (var item in order.OrderDetails)
        {
            order.TotalAmount += item.Price * (DateTime.UtcNow - order.StartKeepTime.Value).TotalHours;
        }

        return order.TotalAmount;
    }

    /// <summary>
    /// Calculate final amount with overtime fees for pickup
    /// </summary>
    public async Task<double> CalculateFinalAmountAsync(Guid orderId)
    {
        try
        {
            var order = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Size)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.StartKeepTime == null)
            {
                return order?.TotalAmount ?? 0;
            }

            // Calculate base amount: Size.Price × EstimatedDays
            double baseAmount = 0;
            foreach (var orderDetail in order.OrderDetails)
            {
                baseAmount += orderDetail.Size!.Price * order.EstimatedDays;
            }

            // Calculate overtime fees (1 hour tolerance)
            var actualHours = (DateTime.UtcNow - order.StartKeepTime.Value).TotalHours;
            var estimatedHours = order.EstimatedDays * 24;
            var toleranceHours = 1; // 1 hour grace period
            var overtimeHours = Math.Max(0, actualHours - estimatedHours - toleranceHours);
            var overtimeFee = overtimeHours * 500; // 500 VND per hour

            var finalAmount = baseAmount + overtimeFee;

            return finalAmount;
        }
        catch (Exception e)
        {
            throw new Exception($"Error calculating final amount: {e.Message}");
        }
    }
    /// <summary>
    /// Update order with certification images
    /// </summary>
    public async Task<bool> UpdateOrderCertificationAsync(Guid orderId, string[] imageUrls)
    {
        try
        {
            if (imageUrls.Length > 2)
            {
                throw new ArgumentException("Maximum 2 certification images allowed");
            }

            _unitOfWork.BeginTransaction();
            var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(orderId);

            if (order == null)
            {
                return false;
            }

            order.OrderCertification = imageUrls;
            _unitOfWork.GetRepository<Order>().Update(order);
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
    public async Task<OrderCountdownModel?> GetOrderCountdownAsync(Guid orderId)
    {
        try
        {
            var order = await _unitOfWork.GetRepository<Order>()
                .Entities
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.StartKeepTime == null)
            {
                return null;
            }

            var startKeepTime = order.StartKeepTime.Value;
            var estimatedEndTime = startKeepTime.AddDays(order.EstimatedDays);
            var currentTime = DateTime.UtcNow;

            var timeRemaining = estimatedEndTime - currentTime;
            var isExpired = timeRemaining.TotalMilliseconds <= 0;

            // Calculate percentage complete
            var totalDuration = estimatedEndTime - startKeepTime;
            var elapsedTime = currentTime - startKeepTime;
            var percentageComplete = Math.Max(0, Math.Min(100, (elapsedTime.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100));

            // Format time remaining
            string formattedTimeRemaining;
            if (isExpired)
            {
                var overdue = currentTime - estimatedEndTime;
                formattedTimeRemaining = $"Overdue by {FormatTimeSpan(overdue)}";
            }
            else
            {
                formattedTimeRemaining = FormatTimeSpan(timeRemaining);
            }

            return new OrderCountdownModel
            {
                OrderId = orderId,
                StartKeepTime = startKeepTime,
                EstimatedDays = order.EstimatedDays,
                EstimatedEndTime = estimatedEndTime,
                TimeRemainingInMilliseconds = (long)timeRemaining.TotalMilliseconds,
                IsExpired = isExpired,
                FormattedTimeRemaining = formattedTimeRemaining,
                PercentageComplete = percentageComplete
            };
        }
        catch (Exception e)
        {
            throw new Exception($"Error calculating countdown for order {orderId}: {e.Message}");
        }
    }
    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        var absoluteTimeSpan = timeSpan.Duration();

        if (absoluteTimeSpan.TotalDays >= 1)
        {
            return $"{(int)absoluteTimeSpan.TotalDays}d {absoluteTimeSpan.Hours}h {absoluteTimeSpan.Minutes}m";
        }
        else if (absoluteTimeSpan.TotalHours >= 1)
        {
            return $"{absoluteTimeSpan.Hours}h {absoluteTimeSpan.Minutes}m";
        }
        else
        {
            return $"{absoluteTimeSpan.Minutes}m {absoluteTimeSpan.Seconds}s";
        }
    }
    public async Task<List<OrderCountdownModel>> GetMultipleOrderCountdownAsync(List<Guid> orderIds)
    {
        try
        {
            var orders = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Where(o => orderIds.Contains(o.Id) && o.StartKeepTime != null)
                .ToListAsync();

            var result = new List<OrderCountdownModel>();
            var currentTime = DateTime.UtcNow;

            foreach (var order in orders)
            {
                var startKeepTime = order.StartKeepTime!.Value;
                var estimatedEndTime = startKeepTime.AddDays(order.EstimatedDays);

                var timeRemaining = estimatedEndTime - currentTime;
                var isExpired = timeRemaining.TotalMilliseconds <= 0;

                // Calculate percentage complete
                var totalDuration = estimatedEndTime - startKeepTime;
                var elapsedTime = currentTime - startKeepTime;
                var percentageComplete = Math.Max(0, Math.Min(100, (elapsedTime.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100));

                // Format time remaining
                string formattedTimeRemaining;
                if (isExpired)
                {
                    var overdue = currentTime - estimatedEndTime;
                    formattedTimeRemaining = $"Overdue by {FormatTimeSpan(overdue)}";
                }
                else
                {
                    formattedTimeRemaining = FormatTimeSpan(timeRemaining);
                }

                result.Add(new OrderCountdownModel
                {
                    OrderId = order.Id,
                    StartKeepTime = startKeepTime,
                    EstimatedDays = order.EstimatedDays,
                    EstimatedEndTime = estimatedEndTime,
                    TimeRemainingInMilliseconds = (long)timeRemaining.TotalMilliseconds,
                    IsExpired = isExpired,
                    FormattedTimeRemaining = formattedTimeRemaining,
                    PercentageComplete = percentageComplete
                });
            }

            return result;
        }
        catch (Exception e)
        {
            throw new Exception($"Error calculating countdown for multiple orders: {e.Message}");
        }
    }
    public async Task<PagingModel<ViewSummaryOrderModel>> GetAllOrderAsync(OrderQuery request)
    {
        try
        {
            var orders = await _unitOfWork.GetRepository<Order>().GetAllAsync();

            if (orders == null || !orders.Any())
            {
                return new PagingModel<ViewSummaryOrderModel>
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    TotalPages = 0,
                    Data = new List<ViewSummaryOrderModel>()
                };
            }

            if (request.IsPaid.HasValue)
            {
                orders = orders.Where(o => o.IsPaid);
            }

            if (request.Status.HasValue)
            {
                orders = orders.Where(o => o.Status == request.Status.Value.ToString());
            }

            if(request.MonthAndYear.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Month == request.MonthAndYear.Value.Month
                && o.OrderDate.Year == request.MonthAndYear.Value.Year);
            }

            var orderView = orders.Select(o => new ViewSummaryOrderModel
            {
                Id = o.Id,
                StorageId = o.StorageId,
                RenterId = o.RenterId,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                PackageDescription = o.PackageDescription,
                OrderDate = o.OrderDate,
                IsPaid = o.IsPaid,
            });

            var pagedData = PagingExtension.ToPagingModel(orderView, request.PageIndex, request.PageSize);

            return pagedData;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}


