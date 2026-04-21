using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Packpal.BLL.Interface;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.Enum;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Services
{
    public class RequestService : IRequestService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IStorageService _storageService;
        private readonly IUserService _userService;

        public RequestService(IMapper mapper, IUnitOfWork uOW, INotificationService notificationService, 
            IStorageService storageService, IUserService userService)
        {
            _mapper = mapper;
            _unitOfWork = uOW;
            _notificationService = notificationService;
            _storageService = storageService;
            _userService = userService;
        }

        public async Task<ViewRequestModel> CreateAsync(CreateRequestModel model)
        {
            model.Type = model.Type.ToUpper();
            var request = _mapper.Map<Request>(model);
            await _unitOfWork.GetRepository<Request>().InsertAsync(request);
            await _unitOfWork.SaveAsync();

            // Get user information for notification
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(model.UserId);
            var viewModel = _mapper.Map<ViewRequestModel>(request);
            
            // Send notification to staff based on request type
            if (user != null)
            {
                if (model.Type == "KEEPER_REGISTRATION")
                {
                    Console.WriteLine($"🔔 [RequestService] Sending keeper registration notification for user: {user.Username}");
                    await _notificationService.NotifyStaffOfKeeperRegistrationAsync(
                        user.Id, 
                        user.Username, 
                        request.Id
                    );
                    Console.WriteLine($"✅ [RequestService] Keeper registration notification sent successfully");
                }
                else if (model.Type == "CREATESTORAGE")
                {
                    Console.WriteLine($"🏠 [RequestService] Sending create storage notification for user: {user.Username}");
                    await _notificationService.NotifyStaffOfCreateStorageAsync(
                        user.Id, 
                        user.Username, 
                        request.Id
                    );
                    Console.WriteLine($"✅ [RequestService] Create storage notification sent successfully");
                }
                else if (model.Type == "DELETESTORAGE")
                {
                    Console.WriteLine($"🗑️ [RequestService] Sending delete storage notification for user: {user.Username}");
                    await _notificationService.NotifyStaffOfDeleteStorageAsync(
                        user.Id, 
                        user.Username, 
                        request.Id
                    );
                    Console.WriteLine($"✅ [RequestService] Delete storage notification sent successfully");
                }
            }
            
            return viewModel;
        }

        public async Task<PagingModel<ViewRequestModel>> GetAllAsync(RequestQuery query)
        {
            try
            {
                var requests = await _unitOfWork.GetRepository<Request>()
                    .Entities
                    .Include(r => r.User)
                    .ToListAsync();

                if (requests == null || !requests.Any())
                {
                    return new PagingModel<ViewRequestModel>
                    {
                        Data = new List<ViewRequestModel>(),
                        TotalCount = 0
                    };
                }

                var filtered = requests.AsQueryable();

                if (!string.IsNullOrEmpty(query.Type.ToString()))
                    filtered = filtered.Where(r => r.Type == query.Type.ToString());

                if (!string.IsNullOrEmpty(query.Status.ToString()))
                    filtered = filtered.Where(r => r.Status == query.Status.ToString());

                if (!string.IsNullOrEmpty(query.Username))
                    filtered = filtered.Where(r => r.User != null && r.User.Username.Contains(query.Username));

                var projected = filtered.Select(r => new ViewRequestModel
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Username = r.User != null ? r.User.Username : string.Empty,
                    Type = r.Type,
                    Status = r.Status,
                    Data = r.Data,
                    RequestedAt = r.RequestedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedBy = r.ReviewedBy
                });

                var paged = PagingExtension.ToPagingModel(projected, query.PageIndex, query.PageSize);
                return paged;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ViewRequestModel?> GetByIdAsync(Guid id)
        {
            var request = await _unitOfWork.GetRepository<Request>()
                .Entities
                .Where(r => Guid.Equals(r.Id, id))
                .Include(r => r.User)
                .FirstOrDefaultAsync();
            return request == null ? null : _mapper.Map<ViewRequestModel>(request);
        }

        public async Task<ViewRequestModel?> UpdateStatusAsync(Guid requestId, Guid userId, RequestStatusEnum status)
        {
            var request = await _unitOfWork.GetRepository<Request>()
                .Entities
                .Where(r => Guid.Equals(r.Id, requestId))
                .Include(r => r.User)
                .FirstOrDefaultAsync();
            if (request == null)
            {
                return null;
            }
            
            var oldStatus = request.Status;
            request.Status = status.ToString();
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedBy = userId;

            _unitOfWork.GetRepository<Request>().Update(request);
            await _unitOfWork.SaveAsync();

            // Send notification to user when status changes
            try
            {
                if (status == RequestStatusEnum.APPROVED || status == RequestStatusEnum.REJECTED)
                {
                    var notification = new
                    {
                        Type = "RequestStatusUpdated",
                        Message = status == RequestStatusEnum.APPROVED 
                            ? $"Your {request.Type} request has been approved!" 
                            : $"Your {request.Type} request has been rejected.",
                        Data = new
                        {
                            RequestId = request.Id,
                            RequestType = request.Type,
                            Status = status.ToString(),
                            UserId = request.UserId,
                            Username = request.User?.Username,
                            Timestamp = DateTime.UtcNow
                        }
                    };

                    await _notificationService.SendToUserAsync(request.UserId.ToString(), "RequestStatusUpdated", notification);
                    
                    // Special handling for approved keeper registration
                    if (status == RequestStatusEnum.APPROVED && request.Type == "KEEPER_REGISTRATION")
                    {
                        var keeperApprovalNotification = new
                        {
                            Type = "KeeperRegistrationApproved",
                            Message = "Congratulations! Your keeper registration has been approved. You can now access keeper features.",
                            Data = new
                            {
                                UserId = request.UserId,
                                RequestId = request.Id,
                                Username = request.User?.Username,
                                Timestamp = DateTime.UtcNow
                            }
                        };

                        await _notificationService.SendToUserAsync(request.UserId.ToString(), "KeeperRegistrationApproved", keeperApprovalNotification);
                    }
                    
                    // Special handling for approved storage creation
                    if (status == RequestStatusEnum.APPROVED && request.Type == "CREATESTORAGE")
                    {
                        try
                        {
                            Console.WriteLine($"🏠 [RequestService] Processing CREATESTORAGE approval for user: {request.UserId}");
                            
                            // Parse storage data from request
                            if (!string.IsNullOrEmpty(request.Data))
                            {
                                var storageData = JsonConvert.DeserializeObject<dynamic>(request.Data);
                                
                                // Get user and their keeper information
                                var userResponse = await _userService.GetAccountDetailAsync(request.UserId.ToString());
                                if (userResponse?.Data?.Keeper?.KeeperId == null)
                                {
                                    Console.WriteLine($"❌ [RequestService] User {request.UserId} is not a keeper or keeper data not found");
                                    return _mapper.Map<ViewRequestModel>(request);
                                }
                                
                                // Create storage model
                                var createStorageModel = new CreateStorageModel
                                {
                                    KeeperId = userResponse.Data.Keeper.KeeperId, // Use keeper ID from user data, NOT the userId parameter!
                                    Description = storageData.description?.ToString() ?? "",
                                    Address = storageData.address?.ToString() ?? "",
                                    Latitude = Convert.ToDouble(storageData.latitude ?? 0),
                                    Longitude = Convert.ToDouble(storageData.longitude ?? 0)
                                };
                                
                                Console.WriteLine($"🏠 [RequestService] Creating storage with KeeperId: {createStorageModel.KeeperId}");
                                
                                // Create the storage
                                var storageId = await _storageService.CreateAsync(createStorageModel);
                                
                                if (storageId != Guid.Empty)
                                {
                                    Console.WriteLine($"✅ [RequestService] Storage created successfully with ID: {storageId}");
                                }
                                else
                                {
                                    Console.WriteLine($"❌ [RequestService] Failed to create storage");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"❌ [RequestService] No storage data found in request");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ [RequestService] Error creating storage: {ex.Message}");
                            // Don't fail the entire request approval if storage creation fails
                        }
                    }
                    
                    // Special handling for approved storage deletion
                    if (status == RequestStatusEnum.APPROVED && request.Type == "DELETESTORAGE")
                    {
                        try
                        {
                            Console.WriteLine($"🗑️ [RequestService] Processing DELETESTORAGE approval for user: {request.UserId}");

                            if (!string.IsNullOrEmpty(request.Data))
                            {
                                var deleteData = JsonConvert.DeserializeObject<dynamic>(request.Data);
                                var storageIdString = deleteData?.storageId?.ToString();

                                Guid storageGuid = Guid.Empty; // <-- ensure initialized
                                if (!string.IsNullOrWhiteSpace(storageIdString) && Guid.TryParse(storageIdString, out storageGuid))
                                {
                                    Console.WriteLine($"🗑️ [RequestService] Deleting storage with ID: {storageGuid}");

                                    var deleteResult = await _storageService.DeleteAsync(storageGuid);

                                    if (deleteResult)
                                    {
                                        Console.WriteLine($"✅ [RequestService] Storage deleted successfully: {storageGuid}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"❌ [RequestService] Failed to delete storage: {storageGuid}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"❌ [RequestService] Invalid storage ID in delete request data: {storageIdString}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"❌ [RequestService] No storage deletion data found in request");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ [RequestService] Error deleting storage: {ex.Message}");
                            // Don't fail the entire request approval if storage deletion fails
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request update
                Console.WriteLine($"❌ Failed to send user notification: {ex.Message}");
            }

            return _mapper.Map<ViewRequestModel>(request);
        }

        public async Task<IEnumerable<ViewRequestModel>> GetByUserIdAsync(Guid userId)
        {
            var requests = await _unitOfWork.GetRepository<Request>()
                .Entities
                .Where(r => r.UserId == userId)
                .Include(r => r.User)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ViewRequestModel>>(requests);
        }
    }
}
