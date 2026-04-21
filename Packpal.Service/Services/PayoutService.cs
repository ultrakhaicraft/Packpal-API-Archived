using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Packpal.BLL.Interface;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.Enum;
using Packpal.DAL.ModelViews;

using static Packpal.DAL.ModelViews.PayoutInfoModel;

namespace Packpal.BLL.Services;

public class PayoutService : IPayoutService
{
	private readonly HttpClient _httpClient;
	private readonly string _secretKey;
	private readonly string _clientId;
	private readonly string _apiKey;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IFirebaseStorageService _fileUploadService;
	private readonly INotificationService _notificationService;


	public PayoutService(HttpClient httpClient, IConfiguration configuration, IUnitOfWork unitOfWork, IFirebaseStorageService fileUploadService, INotificationService notificationService)
	{
		_httpClient = httpClient;
		_secretKey = configuration["PayOSConfig:SecretKey"]!;
		_clientId = configuration["PayOSConfig:ClientId"]!;
		_apiKey = configuration["PayOSConfig:ApiKey"]!;
		_unitOfWork = unitOfWork;
		_fileUploadService = fileUploadService;
		_notificationService = notificationService;
	}

	/// <summary>
	/// Create Payout and save it to database
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	public async Task<BaseResponseModel<ViewPayoutInfo>> CreatePayout (CreatePayoutInfo request)
	{
		try
		{
			_unitOfWork.BeginTransaction();
			
			//Create new payout request and save to database
			var payoutAmount = await GetPayoutAmount(request.OrderId);

			if(payoutAmount == 0.0)
			{
				return BaseResponseModel<ViewPayoutInfo>.NotFoundResponseModel(null, message: "Order not found while getting payout amount");
			}

			// Validate order exists and is paid
			var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(request.OrderId);
			if (order == null)
			{
				return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, message: "Order not found");
			}

			if (!order.IsPaid)
			{
				return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, message: "Order must be paid before creating payout request");
			}

			// Validate keeper ownership của order
			var storage = await _unitOfWork.GetRepository<Storage>().GetByIdAsync(order.StorageId);
			if (storage == null || storage.KeeperId != request.KeeperId)
			{
				return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, message: "Keeper does not own this order's storage");
			}

			// Check if payout request already exists for this order
			var existingPayout = await _unitOfWork.GetRepository<PayoutRequest>()
				.Entities
				.FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
			
			if (existingPayout != null)
			{
				return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, message: "Payout request already exists for this order");
			}

			var payoutRequest = new PayoutRequest
			{
				OrderId = request.OrderId,
				UserId = null, // Sẽ được set khi staff xử lý
				Amount = payoutAmount,
				Status = PayoutRequestStatusEnum.NOTPAID.ToString()
			};

			await _unitOfWork.GetRepository<PayoutRequest>().InsertAsync(payoutRequest);
			await _unitOfWork.SaveAsync();
			_unitOfWork.CommitTransaction();

			var payoutInfoResponse = new ViewPayoutInfo
			{
				Id = payoutRequest.Id,
				KeeperId = request.KeeperId,
				OrderId = payoutRequest.OrderId,
				UserId = payoutRequest.UserId,
				TransactionId = payoutRequest.TransactionId,
				Amount = payoutRequest.Amount,
				CreatedAt = payoutRequest.CreatedAt,
				Status = payoutRequest.Status
			};


			return BaseResponseModel<ViewPayoutInfo>.
				OkResponseModel(payoutInfoResponse, message: "Payout completed successfully"); 
		}
		catch (Exception ex)
		{

			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<ViewPayoutInfo>.
				InternalErrorResponseModel(null, message: "Server caught an error when performing payout" + ex.Message);
			return errorRes;
		}
	}

	/// <summary>
	/// Staff starts processing a payout request - sets status to BUSY and assigns staff user ID
	/// </summary>
	/// <param name="payoutId"></param>
	/// <param name="staffUserId"></param>
	/// <returns></returns>
	public async Task<BaseResponseModel<ViewPayoutInfo>> StartProcessingPayout(Guid payoutId, Guid staffUserId)
	{
		try
		{
			var payoutRequest = await _unitOfWork.GetRepository<PayoutRequest>()
				.Entities
				.Include(p => p.Order)
					.ThenInclude(o => o!.Storage)
					.ThenInclude(s => s!.Keeper)
					.ThenInclude(k => k!.User)
				.FirstOrDefaultAsync(p => p.Id == payoutId);

			if (payoutRequest == null)
			{
				return BaseResponseModel<ViewPayoutInfo>.NotFoundResponseModel(null, "Payout request not found with the ID");
			}

			if (payoutRequest.Status == PayoutRequestStatusEnum.PAID.ToString())
			{
				return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, "This payout request is already paid, can't update");
			}

			// Update status to BUSY and set staff user ID
			payoutRequest.Status = PayoutRequestStatusEnum.BUSY.ToString();
			payoutRequest.UserId = staffUserId; // Set staff ID who is processing this request

			await _unitOfWork.GetRepository<PayoutRequest>().UpdateAsync(payoutRequest);
			await _unitOfWork.SaveAsync();

			var keeper = payoutRequest.Order?.Storage?.Keeper;
			var payoutInfoResponse = new ViewPayoutInfo
			{
				Id = payoutRequest.Id,
				OrderId = payoutRequest.OrderId,
				UserId = payoutRequest.UserId,
				KeeperId = keeper?.Id, // Get keeperId through Order -> Storage -> Keeper
				TransactionId = payoutRequest.TransactionId,
				Amount = payoutRequest.Amount,
				CreatedAt = payoutRequest.CreatedAt,
				Status = payoutRequest.Status,
				ImageUrl = payoutRequest.ImageURL,
				Keeper = keeper != null ? new KeeperInfo
				{
					Username = keeper.User?.Username ?? "",
					Email = keeper.User?.Email ?? "",
					BankAccount = keeper.BankAccount ?? "",
					FullName = keeper.User?.Username ?? "" // Use Username as FullName since FullName doesn't exist
				} : null
			};

			return BaseResponseModel<ViewPayoutInfo>.OkResponseModel(payoutInfoResponse, message: "Payout processing started successfully by staff");
		}
		catch (Exception ex)
		{
			_unitOfWork.RollBack();
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<ViewPayoutInfo>.
				InternalErrorResponseModel(null, message: "Server caught an error when starting payout processing: " + ex.Message);
			return errorRes;
		}
	}

	/// <summary>
	/// Change the payout status, used to change the status to NOTPAID, BUSY OR COMPLETED (working on a guard to not set COMPLETED)
	/// </summary>
	/// <param name="payoutId"></param>
	/// <param name="status"></param>
	/// <returns></returns>
	public async Task<BaseResponseModel<ViewPayoutInfo>> ChangePayoutStatus (Guid payoutId, PayoutRequestStatusEnum status)
	{
		try
		{
		if(status == PayoutRequestStatusEnum.PAID)
		{
			return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, "Can't change this status to PAID through this method, please use CompletePayout method instead");
		}

		var payoutRequest = await _unitOfWork.GetRepository<PayoutRequest>()
			.Entities
			.Include(p => p.Order)
				.ThenInclude(o => o!.Storage)
				.ThenInclude(s => s!.Keeper)
				.ThenInclude(k => k!.User)
			.FirstOrDefaultAsync(p => p.Id == payoutId);
		if (payoutRequest == null)
		{
			return BaseResponseModel<ViewPayoutInfo>.NotFoundResponseModel(null, "Payout request not found with the ID");
		}			if(payoutRequest.Status== PayoutRequestStatusEnum.PAID.ToString())
			{
				return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, "This payout request is already paid, can't update");

			}

		payoutRequest.Status = status.ToString();
		await _unitOfWork.GetRepository<PayoutRequest>().UpdateAsync(payoutRequest);
		await _unitOfWork.SaveAsync();

		var keeper = payoutRequest.Order?.Storage?.Keeper;
		var payoutInfoResponse = new ViewPayoutInfo
		{
			Id = payoutRequest.Id,
			OrderId = payoutRequest.OrderId,
			UserId = payoutRequest.UserId,
			KeeperId = keeper?.Id, // Include keeper ID
			TransactionId = payoutRequest.TransactionId,
			Amount = payoutRequest.Amount,
			CreatedAt = payoutRequest.CreatedAt,
			Status = payoutRequest.Status,
			ImageUrl = payoutRequest.ImageURL,
			Keeper = keeper != null ? new KeeperInfo
			{
				Username = keeper.User?.Username ?? "",
				Email = keeper.User?.Email ?? "",
				BankAccount = keeper.BankAccount ?? "",
				FullName = keeper.User?.Username ?? "" // Use Username as FullName
			} : null
		};			return BaseResponseModel<ViewPayoutInfo>.OkResponseModel(payoutInfoResponse, message: "Payout status updated successfully");
		}
		catch (Exception ex)
		{
			_unitOfWork.RollBack();
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<ViewPayoutInfo>.
				InternalErrorResponseModel(null, message: "Server caught an error when changing payout status" + ex.Message);
			return errorRes;
		}
	}

	
	public async Task<BaseResponseModel<ViewPayoutInfo>> UploadProofToPayout(Guid payoutId, IFormFile proofImage)
	{
		try
		{
		Console.WriteLine("Now at the Payout service");
		var payoutRequest = await _unitOfWork.GetRepository<PayoutRequest>()
			.Entities
			.Include(p => p.Order)
				.ThenInclude(o => o!.Storage)
				.ThenInclude(s => s!.Keeper)
				.ThenInclude(k => k!.User)
			.FirstOrDefaultAsync(p => p.Id == payoutId);
		if (payoutRequest == null)
		{
			return BaseResponseModel<ViewPayoutInfo>.NotFoundResponseModel(null, "Payout request not found with the ID");
		}			if (payoutRequest.Status == PayoutRequestStatusEnum.PAID.ToString())
			{
				return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, "This payout request is already paid, can't update");

			}

			UploadResponse uploadResult = await _fileUploadService.UploadFileAsync(proofImage);
			string proofUrl = uploadResult.FileUrl;

		payoutRequest.ImageURL= proofUrl;

		await _unitOfWork.GetRepository<PayoutRequest>().UpdateAsync(payoutRequest);
		await _unitOfWork.SaveAsync();

		var keeper = payoutRequest.Order?.Storage?.Keeper;
		var payoutInfoResponse = new ViewPayoutInfo
		{
			Id = payoutRequest.Id,
			OrderId = payoutRequest.OrderId,
			UserId = payoutRequest.UserId,
			KeeperId = keeper?.Id, // Include keeper ID
			TransactionId = payoutRequest.TransactionId,
			Amount = payoutRequest.Amount,
			CreatedAt = payoutRequest.CreatedAt,
			Status = payoutRequest.Status,
			ImageUrl = payoutRequest.ImageURL,
			Keeper = keeper != null ? new KeeperInfo
			{
				Username = keeper.User?.Username ?? "",
				Email = keeper.User?.Email ?? "",
				BankAccount = keeper.BankAccount ?? "",
				FullName = keeper.User?.Username ?? "" // Use Username as FullName
			} : null
		};			return BaseResponseModel<ViewPayoutInfo>.OkResponseModel(payoutInfoResponse, message: "Payout status updated successfully");
		}
		catch (Exception ex)
		{
			_unitOfWork.RollBack();
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<ViewPayoutInfo>.
				InternalErrorResponseModel(null, message: "Server caught an error when changing payout status" + ex.Message);
			return errorRes;
		}
	}
	

	public async Task<BaseResponseModel<ViewPayoutInfo>> CompletePayout(Guid payoutId, string transactionCode, string description)
	{
	try
	{
		_unitOfWork.BeginTransaction();
		var payoutRequest = await _unitOfWork.GetRepository<PayoutRequest>()
			.Entities
			.Include(p => p.Order)
				.ThenInclude(o => o!.Storage)
				.ThenInclude(s => s!.Keeper)
				.ThenInclude(k => k!.User)
			.FirstOrDefaultAsync(p => p.Id == payoutId);
		if (payoutRequest == null)
		{
			return BaseResponseModel<ViewPayoutInfo>.NotFoundResponseModel(null, "Payout request not found with the ID");
		}			if (payoutRequest.Status == PayoutRequestStatusEnum.PAID.ToString())
			{
				return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, "This payout request is already paid, can't update");

			}

			if (payoutRequest.Status != PayoutRequestStatusEnum.BUSY.ToString())
			{
				return BaseResponseModel<ViewPayoutInfo>.BadRequestResponseModel(null, "This payout request is not being handle, therefore can't complete this request");
			}

			if(payoutRequest.ImageURL.IsNullOrEmpty())
			{
				return BaseResponseModel<ViewPayoutInfo>.NotFoundResponseModel(null, "Payout request does not have proof image, therefore can't complete this request");
			}

			//Create new transaction and save to database once Payout is completed
			var transaction = new DAL.Entity.Transaction
			{
				OrderId = payoutRequest.OrderId,
				TransactionCode = transactionCode,
				Amount = payoutRequest.Amount,
				Status = DAL.Enum.TransactionStatus.COMPLETED.ToString(),
				Description = description,
				TransactionType = TransactionTypeEnum.OUT.ToString(),
			};

			await _unitOfWork.GetRepository<DAL.Entity.Transaction>().InsertAsync(transaction);

			//Update PayoutRequest status to COMPLETED
			

			payoutRequest.TransactionId = transaction.Id;
			payoutRequest.Status = PayoutRequestStatusEnum.PAID.ToString();

			await _unitOfWork.GetRepository<PayoutRequest>().UpdateAsync(payoutRequest);
			await _unitOfWork.SaveAsync();
			 _unitOfWork.CommitTransaction();

		// Notify keeper that payout has been completed
		// Get keeper ID from the order storage instead of using payoutRequest.UserId (which is staff ID)
		var keeper = payoutRequest.Order?.Storage?.Keeper;
		if (keeper != null)
		{
			await _notificationService.NotifyKeeperPayoutCompletedAsync(
				keeper.Id, 
				(decimal)payoutRequest.Amount, 
				payoutRequest.Id
			);
		}

		var payoutInfoResponse = new ViewPayoutInfo
		{
			Id = payoutRequest.Id,
			OrderId = payoutRequest.OrderId,
			UserId = payoutRequest.UserId,
			KeeperId = keeper?.Id, // Include keeper ID
			TransactionId = payoutRequest.TransactionId,
			Amount = payoutRequest.Amount,
			CreatedAt = payoutRequest.CreatedAt,
			Status = payoutRequest.Status,
			ImageUrl = payoutRequest.ImageURL,
			Keeper = keeper != null ? new KeeperInfo
			{
				Username = keeper.User?.Username ?? "",
				Email = keeper.User?.Email ?? "",
				BankAccount = keeper.BankAccount ?? "",
				FullName = keeper.User?.Username ?? "" // Use Username as FullName
			} : null
		};			
			return BaseResponseModel<ViewPayoutInfo>.
				OkResponseModel(payoutInfoResponse, message: "Payout completed successfully"); ;

		}
		catch (Exception ex)
		{
			_unitOfWork.RollBack();
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<ViewPayoutInfo>.
				InternalErrorResponseModel(null, message: "Server caught an error when performing completing payout" + ex.Message);
			return errorRes;
		}
	}

	public async Task<BaseResponseModel<ViewPayoutInfo>> GetAPayoutInfo(Guid payoutId)
	{
	try
	{

		var payoutRequest = await _unitOfWork.GetRepository<PayoutRequest>()
			.Entities
			.Include(p => p.Order)
				.ThenInclude(o => o!.Storage)
				.ThenInclude(s => s!.Keeper)
				.ThenInclude(k => k!.User)
			.FirstOrDefaultAsync(p => p.Id == payoutId);
		if (payoutRequest == null)
		{
			return BaseResponseModel<ViewPayoutInfo>.NotFoundResponseModel(null, "Payout request not found with the ID");
		}

		var keeper = payoutRequest.Order?.Storage?.Keeper;
		var payoutInfoResponse = new ViewPayoutInfo
		{
			Id = payoutRequest.Id,
			OrderId = payoutRequest.OrderId,
			UserId = payoutRequest.UserId,
			KeeperId = keeper?.Id, // Include keeper ID
			TransactionId = payoutRequest.TransactionId,
			Amount = payoutRequest.Amount,
			CreatedAt = payoutRequest.CreatedAt,
			Status = payoutRequest.Status,
			ImageUrl = payoutRequest.ImageURL,
			Keeper = keeper != null ? new KeeperInfo
			{
				Username = keeper.User?.Username ?? "",
				Email = keeper.User?.Email ?? "",
				BankAccount = keeper.BankAccount ?? "",
				FullName = keeper.User?.Username ?? "" // Use Username as FullName
			} : null
		};
			return BaseResponseModel<ViewPayoutInfo>.
				OkResponseModel(payoutInfoResponse, message: "Get Payout by ID successfully"); ;

		}
		catch (Exception ex)
		{

			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<ViewPayoutInfo>.
				InternalErrorResponseModel(null, message: "Server caught an error when getting a payout" + ex.Message);
			return errorRes;
		}
	}

	public async Task<BaseResponseModel<PagingModel<ViewPayoutInfo>>> GetAllPayoutRequests(int pageIndex, int pageSize, string? status)
	{
		try
		{
			var payoutRequests = await _unitOfWork.GetRepository<PayoutRequest>()
				.Entities
				.Include(p => p.Order)
					.ThenInclude(o => o!.Storage)
						.ThenInclude(s => s!.Keeper)
							.ThenInclude(k => k!.User)
				.ToListAsync();

			// Filter by status if provided
			if (!string.IsNullOrEmpty(status))
			{
				payoutRequests = payoutRequests.Where(p => p.Status == status).ToList();
			}

		// Convert to ViewPayoutInfo
		var payoutInfos = payoutRequests.Select(p => {
			var keeper = p.Order?.Storage?.Keeper;
			return new ViewPayoutInfo
			{
				Id = p.Id,
				OrderId = p.OrderId,
				UserId = p.UserId,
				KeeperId = keeper?.Id,
				TransactionId = p.TransactionId,
				Amount = p.Amount,
				CreatedAt = p.CreatedAt,
				Status = p.Status,
				ImageUrl = p.ImageURL,
				Keeper = keeper != null ? new KeeperInfo
				{
					Username = keeper.User?.Username ?? "",
					Email = keeper.User?.Email ?? "",
					BankAccount = keeper.BankAccount ?? "",
					FullName = keeper.User?.Username ?? "" // Use Username as FullName
				} : null
			};
		}).ToList();			// Apply pagination
			var pagedData = PagingExtension.ToPagingModel(payoutInfos, pageIndex, pageSize);

			return BaseResponseModel<PagingModel<ViewPayoutInfo>>.OkResponseModel(pagedData, "Payout requests retrieved successfully");
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return BaseResponseModel<PagingModel<ViewPayoutInfo>>.InternalErrorResponseModel(null, "Server error when getting payout requests: " + ex.Message);
		}
	}

	public async Task<BaseResponseModel<PagingModel<ViewPayoutInfo>>> GetPayoutRequestsByKeeper(Guid keeperId, int pageIndex, int pageSize)
	{
		try
		{
		var payoutRequests = await _unitOfWork.GetRepository<PayoutRequest>()
			.Entities
			.Include(p => p.Order)
				.ThenInclude(o => o!.Storage)
				.ThenInclude(s => s!.Keeper)
				.ThenInclude(k => k!.User)
			.Where(p => p.Order!.Storage!.KeeperId == keeperId)
			.OrderByDescending(p => p.CreatedAt)
			.ToListAsync();

		// Convert to ViewPayoutInfo
		var payoutInfos = payoutRequests.Select(p => {
			var keeper = p.Order?.Storage?.Keeper;
			return new ViewPayoutInfo
			{
				Id = p.Id,
				OrderId = p.OrderId,
				UserId = p.UserId,
				KeeperId = keeperId,
				TransactionId = p.TransactionId,
				Amount = p.Amount,
				CreatedAt = p.CreatedAt,
				Status = p.Status,
				ImageUrl = p.ImageURL,
				Keeper = keeper != null ? new KeeperInfo
				{
					Username = keeper.User?.Username ?? "",
					Email = keeper.User?.Email ?? "",
					BankAccount = keeper.BankAccount ?? "",
					FullName = keeper.User?.Username ?? "" // Use Username as FullName
				} : null
			};
		}).ToList();			// Apply pagination
			var pagedData = PagingExtension.ToPagingModel(payoutInfos, pageIndex, pageSize);

			return BaseResponseModel<PagingModel<ViewPayoutInfo>>.OkResponseModel(pagedData, "Keeper payout requests retrieved successfully");
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return BaseResponseModel<PagingModel<ViewPayoutInfo>>.InternalErrorResponseModel(null, "Server error when getting keeper payout requests: " + ex.Message);
		}
	}

	public async Task<BaseResponseModel<object>> CheckPayoutEligibility(Guid orderId, Guid keeperId)
	{
		try
		{
			var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(orderId);
			if (order == null)
			{
				return BaseResponseModel<object>.NotFoundResponseModel(null, "Order not found");
			}

			var storage = await _unitOfWork.GetRepository<Storage>().GetByIdAsync(order.StorageId);
			if (storage == null || storage.KeeperId != keeperId)
			{
				return BaseResponseModel<object>.BadRequestResponseModel(null, "Keeper does not own this order's storage");
			}

			if (!order.IsPaid)
			{
				return BaseResponseModel<object>.BadRequestResponseModel(null, "Order must be paid before creating payout request");
			}

			// Check if payout request already exists
			var existingPayout = await _unitOfWork.GetRepository<PayoutRequest>()
				.Entities
				.FirstOrDefaultAsync(p => p.OrderId == orderId);

			if (existingPayout != null)
			{
				return BaseResponseModel<object>.BadRequestResponseModel(null, "Payout request already exists for this order");
			}

			// Calculate payout amount
			var payoutAmount = await GetPayoutAmount(orderId);

			var eligibilityInfo = new
			{
				IsEligible = true,
				OrderId = orderId,
				PayoutAmount = payoutAmount,
				Commission = order.TotalAmount - payoutAmount,
				OrderTotal = order.TotalAmount,
				Message = "Order is eligible for payout request"
			};

			return BaseResponseModel<object>.OkResponseModel(eligibilityInfo, "Payout eligibility checked successfully");
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return BaseResponseModel<object>.InternalErrorResponseModel(null, "Server error when checking payout eligibility: " + ex.Message);
		}
	}

	public async Task<BaseResponseModel<object>> GetOrderPayoutStatus(Guid orderId, Guid keeperId)
	{
		try
		{
			var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(orderId);
			if (order == null)
			{
				return BaseResponseModel<object>.NotFoundResponseModel(null, "Order not found");
			}

			var storage = await _unitOfWork.GetRepository<Storage>().GetByIdAsync(order.StorageId);
			if (storage == null || storage.KeeperId != keeperId)
			{
				return BaseResponseModel<object>.BadRequestResponseModel(null, "Keeper does not own this order's storage");
			}

			// Check if payout request exists
			var existingPayout = await _unitOfWork.GetRepository<PayoutRequest>()
				.Entities
				.FirstOrDefaultAsync(p => p.OrderId == orderId);

			var statusInfo = new
			{
				OrderId = orderId,
				HasPayoutRequest = existingPayout != null,
				PayoutStatus = existingPayout?.Status,
				PayoutId = existingPayout?.Id,
				PayoutAmount = existingPayout?.Amount,
				IsPaid = order.IsPaid,
				CanCreatePayout = order.IsPaid && existingPayout == null,
				IsPayoutCompleted = existingPayout?.Status == PayoutRequestStatusEnum.PAID.ToString(),
				Message = existingPayout == null 
					? (order.IsPaid ? "Order is eligible for payout request" : "Order must be paid before creating payout request")
					: $"Payout request already exists with status: {existingPayout.Status}"
			};

			return BaseResponseModel<object>.OkResponseModel(statusInfo, "Order payout status retrieved successfully");
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return BaseResponseModel<object>.InternalErrorResponseModel(null, "Server error when getting order payout status: " + ex.Message);
		}
	}

	private async Task<double> GetPayoutAmount(Guid orderId)
	{
		var order=  await _unitOfWork.GetRepository<Order>().GetByIdAsync(orderId);
		if (order == null)
		{
			return 0.0;
		}

		var payoutAmount = GeneralHelper.CalculateComissionFee(order.TotalAmount, 20);

		return payoutAmount.AmountLeft;
	}

	private async Task<Guid> GetKeeperIdFromStorage(Guid storageId)
	{
		var storage = await _unitOfWork.GetRepository<Storage>().GetByIdAsync(storageId);
		if (storage == null)
		{
			return Guid.Empty;
		}

		var keeper = await _unitOfWork.GetRepository<Keeper>().GetByIdAsync(storage.KeeperId);
		if (keeper == null)
		{
			return Guid.Empty;
		}

		return keeper.Id;
	}

	/*
	private static string GenerateHmacSHA256(string dataStr, string key)
	{
		using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes(key));
		byte[] array = hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(dataStr));
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}

		return stringBuilder.ToString();
	}

	public static string CreateSignatureOfPayout(PayOutRequest data, string key)
	{
		int amount = data.Amount;
		string category= string.Join(",", data.Category ?? Array.Empty<string>());
		string description = data.Description;
		string referenceId = data.ReferenceId;
		string toAccountNumber = data.ToAccountNumber;
		string toBin = data.ToBin;
		return GenerateHmacSHA256($"amount={amount}&category={category}&description={description}&referenceId={referenceId}&toAccountNumber={toAccountNumber}&toBin={toBin}"
			, key);

	}

	*/

	
}
