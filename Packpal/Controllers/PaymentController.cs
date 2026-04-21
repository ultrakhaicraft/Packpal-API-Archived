using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using Packpal.BLL.Interface;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.Enum;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : Controller
{
	
	private readonly PayOS _payOS;
	private readonly ITransactionService _transactionService;
	private readonly IOrderService _orderService;
	private readonly INotificationService _notificationService;
	private readonly IUnitOfWork _unitOfWork;
	
	public PaymentController( PayOS payOS, ITransactionService transactionService, IOrderService orderService, INotificationService notificationService, IUnitOfWork unitOfWork)
	{
		
		_payOS = payOS;
		_transactionService = transactionService;
		_orderService = orderService;
		_notificationService = notificationService;
		_unitOfWork = unitOfWork;
		
	}

	/// <summary>
	/// Create Link for payment
	/// </summary>
	/// <remarks>Please keep the OrderId in the Frontend for the success and failure callback API. Also OrderCode/PaymentCode will be automically generated</remarks>
	/// <param name="request">Note: PaymentCode only accept number because it's Long Type</param>
	/// <returns></returns>
	[Authorize]
	[HttpPost("create-link")]
	public async Task<IActionResult> CreateLink([FromBody] PaymentRequest request)
	{
		try
		{
			
			long longOrderCode = Helper.Utility.GenerateSecureOrderCode(); // This is just to ensure the utility is used, you can remove it if not needed.;
			var itemList = new List<ItemData>();
			var item = new ItemData(request.OrderId,1,request.Amount);
			itemList.Add(item);
			PaymentData paymentData = new PaymentData(longOrderCode, request.Amount, request.Description, itemList, request.CancelUrl, request.ReturnUrl);
			var result = await _payOS.createPaymentLink(paymentData);

			// Wrap the result in BaseResponseModel for consistent API response format
			var response = BaseResponseModel<object>.OkResponseModel(
				data: result, 
				message: "Payment link created successfully"
			);
			
			return Ok(response);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<PaymentData>.InternalErrorResponseModel(null, message: "Server caught an error when creating link " + ex.Message);
			return StatusCode(500, errorRes);
		}
	}

	/// <summary>
	/// Create a transaction in the database, typically used for creating a transaction after the payment link is created.
	/// 
	/// </summary>
	/// <remarks>Please keep the TransactionId for later use, such as success or failure callback.</remarks>
	/// <param name="request"></param>
	/// <returns></returns>
	[Authorize]
	[HttpPost("create-transaction")]
	public async Task<IActionResult> CreateTransaction([FromBody] TransactionCreateModel request)
	{
		try
		{
			
			var response = await _transactionService.Create(request);
			if (response.StatusCode != StatusCodes.Status200OK)
			{
				return StatusCode(response.StatusCode, response);
			}
			return Ok(response);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<string>.InternalErrorResponseModel(null, message: "Server caught an error when creating transaction: " + ex.Message);
			return StatusCode(500, errorRes);
		}
	}

	/// <summary>
	/// Get Order Information by Payment Code Id
	/// </summary>
	/// <remarks>PaymentCodeId is the Payment Code when you </remarks>
	/// <param name="paymentCodeId"></param>
	/// <returns></returns>
	[Authorize]
	[HttpGet("{paymentCodeId}")]
	public async Task<IActionResult> GetOrder([FromRoute] long paymentCodeId)
	{
		try
		{
			PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(paymentCodeId);
			var response = BaseResponseModel<PaymentLinkInformation>.OkResponseModel(data:paymentLinkInformation, message: "Retrieving payment info");
			return Ok(response);
		}
		catch (Exception ex)
		{

			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<PaymentLinkInformation>.InternalErrorResponseModel(null, message: "Server caught an error when getting info: " + ex.Message);
			return StatusCode(500, errorRes);
		}

	}

	/// <summary>
	/// Cancel Order by Payment Code Id AND Transaction Id
	/// </summary>
	/// <param name="paymentCodeId"></param>
	/// <param name="transactionId"></param>
	/// <returns></returns>
	[Authorize]
	[HttpPatch("cancel-order")]
	public async Task<IActionResult> CancelOrder([FromQuery] long paymentCodeId, string transactionId)
	{
		try
		{
			PaymentLinkInformation paymentLinkInformation = await _payOS.cancelPaymentLink(paymentCodeId);
			await _transactionService.ChangeStatus(transactionId, TransactionStatus.CANCELLED.ToString());
			var response = BaseResponseModel<PaymentLinkInformation>.OkResponseModel(data: paymentLinkInformation, message: "Cancelling Order completed");
			return Ok(response);
		}
		catch (Exception ex)
		{

			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<PaymentLinkInformation>.InternalErrorResponseModel(null, message: "Server caught an error when cancelling: " + ex.Message);
			return StatusCode(500, errorRes);
		}

	}

	/// <summary>
	/// Update the payment/transaction status based on the Payment Code Id and Transaction Id.
	/// Enhanced to handle complete payment flow including order status and notifications.
	/// </summary>
	/// <param name="paymentCodeId"></param>
	/// <param name="transactionId"></param>
	/// <returns></returns>
	[Authorize]
	[HttpPost("update-payment-status")]
	public async Task<IActionResult> UpdatePaymentStatus([FromQuery] long paymentCodeId, string transactionId)
	{
		try
		{
			BaseResponseModel<string> response;
			PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(paymentCodeId);

			if (paymentLinkInformation.status == "PAID")
			{
				// Update transaction status to completed
				await _transactionService.ChangeStatus(transactionId, TransactionStatus.COMPLETED.ToString());

				// Get transaction to find associated order
				var transactionResult = await _transactionService.GetById(transactionId);
				if (transactionResult?.Data?.OrderId != null)
				{
					var orderId = new Guid(transactionResult.Data.OrderId);

					// Mark order as paid
					await _orderService.UpdateIsPaidAsync(orderId);

					// Update order status to COMPLETED after successful payment
					Console.WriteLine($"[Update Payment Status] Updating order status to COMPLETED for order {orderId}");
					await _orderService.UpdateOrderStatusAsync(orderId, "COMPLETED");

					// Get order details for notifications
					var orderDetails = await _orderService.GetOrderDetailByIdAsync(orderId);
					if (orderDetails != null)
					{
						try
						{
							// Get storage information to find keeper ID
							var storage = await _unitOfWork.GetRepository<Storage>()
								.Entities
								.Include(s => s.Keeper)
									.ThenInclude(k => k!.User)
								.FirstOrDefaultAsync(s => s.Id == orderDetails.StorageId);

							if (storage?.Keeper != null)
							{
								// Notify keeper about successful payment and completion
								await _notificationService.NotifyKeeperOrderStatusChangeAsync(
									storage.KeeperId,
									orderId,
									"COMPLETED"
								);

								// Notify renter about payment confirmation and completion
								await _notificationService.NotifyRenterOrderStatusChangeAsync(
									orderDetails.RenterId,
									orderId,
									"COMPLETED",
									storage.Keeper.User?.Username ?? "Storage Keeper"
								);
							}
						}
						catch (Exception notifyEx)
						{
							Console.WriteLine($"Failed to send payment success notifications: {notifyEx.Message}");
							// Don't fail the payment update if notification fails
						}
					}
				}
			}
			else if (paymentLinkInformation.status == "FAILED")
				await _transactionService.ChangeStatus(transactionId, TransactionStatus.FAILED.ToString());
			else if (paymentLinkInformation.status == "CANCELLED")
			{
				await _transactionService.ChangeStatus(transactionId, TransactionStatus.CANCELLED.ToString());
			}

			response = BaseResponseModel<string>.OkResponseModel(data: "", message: "Update payment status complete");
			return StatusCode(response.StatusCode, response);

		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			var errorRes = BaseResponseModel<PaymentLinkInformation>.
				InternalErrorResponseModel(null, message: "Server caught an error when updating payment status: " + ex.Message);
			return StatusCode(500, errorRes);
		}
	}

	/// <summary>
	/// PayOS webhook endpoint to handle payment status updates
	/// </summary>
	[HttpPost("os/webhook")]
	public async Task<IActionResult> PayOSWebhook()
	{
		try
		{
			// Read raw body as string first
			using var reader = new StreamReader(Request.Body);
			var body = await reader.ReadToEndAsync();
			Console.WriteLine($"[PayOS Webhook] Received raw body: {body}");

			// Handle empty body (test webhook from PayOS dashboard)
			if (string.IsNullOrEmpty(body))
			{
				Console.WriteLine("[PayOS Webhook] Empty body received - likely test webhook");
				return Ok(new { message = "Webhook endpoint is working" });
			}

			// Parse JSON
			var webhookData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);
			
			// Check if this is a test webhook (PayOS dashboard test)
			if (webhookData.TryGetProperty("test", out _))
			{
				Console.WriteLine("[PayOS Webhook] Test webhook received from PayOS dashboard");
				return Ok(new { message = "Test webhook received successfully" });
			}

			// Handle real payment webhook
			if (!webhookData.TryGetProperty("data", out var dataElement))
			{
				Console.WriteLine("[PayOS Webhook] Missing 'data' property in webhook");
				return Ok(new { message = "Webhook received but no data to process" });
			}

			if (!dataElement.TryGetProperty("orderCode", out var orderCodeElement) ||
			    !dataElement.TryGetProperty("code", out var statusCodeElement))
			{
				Console.WriteLine("[PayOS Webhook] Missing orderCode or code in webhook data");
				return Ok(new { message = "Webhook received but missing required fields" });
			}

			var orderCode = orderCodeElement.GetInt64().ToString();
			var statusCode = statusCodeElement.GetString();
			
			Console.WriteLine($"[PayOS Webhook] Processing: OrderCode={orderCode}, StatusCode={statusCode}");

			// Find transaction by PayOS order code or try to find by order description
			var transaction = await _unitOfWork.GetRepository<Packpal.DAL.Entity.Transaction>()
				.Entities
				.FirstOrDefaultAsync(t => t.TransactionCode == orderCode);

			// If not found by orderCode, try to find the most recent PENDING transaction for this order description
			if (transaction == null && dataElement.TryGetProperty("description", out var descriptionElement))
			{
				var description = descriptionElement.GetString();
				Console.WriteLine($"[PayOS Webhook] Searching by description: {description}");
				
				// Extract orderId from description if it contains order pattern
				if (!string.IsNullOrEmpty(description))
				{
					// Try to find pending transactions that might match
					transaction = await _unitOfWork.GetRepository<Packpal.DAL.Entity.Transaction>()
						.Entities
						.Where(t => t.Status == TransactionStatus.PENDING.ToString() && 
								   t.Description.Contains("PackPals"))
						.OrderByDescending(t => t.CreatedAt)
						.FirstOrDefaultAsync();
						
					if (transaction != null)
					{
						Console.WriteLine($"[PayOS Webhook] Found pending transaction by description: {transaction.Id}");
						// Update transaction code to match PayOS order code for future webhooks
						transaction.TransactionCode = orderCode;
						await _unitOfWork.SaveAsync();
					}
				}
			}

			if (transaction == null)
			{
				Console.WriteLine($"[PayOS Webhook] Transaction not found for order code: {orderCode}");
				// Return OK to prevent PayOS from retrying, but log for debugging
				return Ok(new { message = "Webhook processed (transaction not found)" });
			}

			// Update transaction status based on PayOS webhook
			string newStatus;
			switch (statusCode)
			{
				case "00": // Payment successful
					newStatus = TransactionStatus.COMPLETED.ToString();
					Console.WriteLine($"[PayOS Webhook] Payment successful for transaction {transaction.Id}");
					
					// Update transaction status
					await _transactionService.ChangeStatus(transaction.Id.ToString(), newStatus);
					
					// Mark order as paid
					await _orderService.UpdateIsPaidAsync(transaction.OrderId);
					
					// Update order status to COMPLETED after successful payment
					Console.WriteLine($"[PayOS Webhook] Updating order status to COMPLETED for order {transaction.OrderId}");
					await _orderService.UpdateOrderStatusAsync(transaction.OrderId, "COMPLETED");
					
					// Get order details for notifications
					var orderDetails = await _orderService.GetOrderDetailByIdAsync(transaction.OrderId);
					if (orderDetails != null)
					{
						try
						{
							// Get storage information to find keeper ID
							var storage = await _unitOfWork.GetRepository<Storage>()
								.Entities
								.Include(s => s.Keeper)
									.ThenInclude(k => k!.User)
								.FirstOrDefaultAsync(s => s.Id == orderDetails.StorageId);

							if (storage?.Keeper != null)
							{
								// Notify keeper about successful payment and completion
								await _notificationService.NotifyKeeperOrderStatusChangeAsync(
									storage.KeeperId,
									transaction.OrderId,
									"COMPLETED"
								);

								// Notify renter about payment confirmation and completion
								await _notificationService.NotifyRenterOrderStatusChangeAsync(
									orderDetails.RenterId,
									transaction.OrderId,
									"COMPLETED",
									storage.Keeper.User?.Username ?? "Storage Keeper"
								);
							}
						}
						catch (Exception notifyEx)
						{
							Console.WriteLine($"[PayOS Webhook] Failed to send payment success notifications: {notifyEx.Message}");
							// Don't fail the payment update if notification fails
						}
					}
					
					break;
					
				case "01": // Payment failed
					newStatus = TransactionStatus.FAILED.ToString();
					Console.WriteLine($"[PayOS Webhook] Payment failed for transaction {transaction.Id}");
					await _transactionService.ChangeStatus(transaction.Id.ToString(), newStatus);
					break;
					
				case "02": // Payment cancelled
					newStatus = TransactionStatus.CANCELLED.ToString();
					Console.WriteLine($"[PayOS Webhook] Payment cancelled for transaction {transaction.Id}");
					await _transactionService.ChangeStatus(transaction.Id.ToString(), newStatus);
					break;
					
				default:
					Console.WriteLine($"[PayOS Webhook] Unknown payment status code: {statusCode}");
					// Return OK to prevent PayOS from retrying
					return Ok(new { message = "Webhook processed (unknown status)" });
			}

			return Ok(new { message = "Webhook processed successfully" });
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[PayOS Webhook] Error processing webhook: {ex.Message}");
			Console.WriteLine(ex.StackTrace);
			// Return OK to prevent PayOS from retrying failed webhooks
			return Ok(new { message = "Webhook received with errors" });
		}
	}
}
