using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packpal.BLL.Interface;
using Packpal.DAL.Constants;
using Packpal.DAL.Enum;
using Packpal.Helper;
using static Packpal.DAL.ModelViews.PayoutInfoModel;
using static System.Net.Mime.MediaTypeNames;

namespace Packpal.Controllers;


[ApiController]
[Route("api/payout")]
public class PayoutController : Controller
{
	private readonly IPayoutService _payoutService;
	private readonly INotificationService _notificationService;

	public PayoutController(IPayoutService payoutService, INotificationService notificationService)
	{
		_payoutService = payoutService;
		_notificationService = notificationService;
	}

	[Authorize(Roles = RoleConstant.KEEPER)]
	[HttpPost("create")]
	public async Task<IActionResult> CreatePayout([FromBody] CreatePayoutInfo request)
	{
		var result = await _payoutService.CreatePayout(request);
		
		// ✅ FIX: Gửi notification khi CREATE THÀNH CÔNG
		if (result.StatusCode == StatusCodes.Status200OK)
		{
			await _notificationService.NotifyStaffOfIncomingPayoutAsync(result.Data!);
			return Ok(result);
		}

		return StatusCode(result.StatusCode, result);
	}

	[Authorize(Roles = RoleConstant.STAFF)]
	[HttpPatch("upload-proof")]
	public async Task<IActionResult> UploadProof([FromForm] Guid payoutId, IFormFile proofImage)
	{
		if (!Utility.IsImageFile(proofImage))
		{
			Console.WriteLine("Caught - Invalid file format. Only Image files are allowed.");
			return BadRequest("Invalid file format. Only Image files are allowed.");
		}
	
			

		if (proofImage.Length > 10 * 1024 * 1024)
		{
			Console.WriteLine("Caught - File size exceeds the limit of 10 MB.");
			return BadRequest("File size exceeds the limit of 10 MB.");
		}
			

		var result = await _payoutService.UploadProofToPayout(payoutId, proofImage);
		if (result.StatusCode != StatusCodes.Status200OK)
		{
			return StatusCode(result.StatusCode, result);
		}
		return Ok(result);
	}

	[Authorize(Roles = RoleConstant.KEEPER_STAFF)]
	[HttpGet("{payoutId}")]
	public async Task<IActionResult> GetPayoutInfo(Guid payoutId)
	{

		var result = await _payoutService.GetAPayoutInfo(payoutId);
		if (result.StatusCode != StatusCodes.Status200OK)
			return StatusCode(result.StatusCode, result);

		return Ok(result);

	}

	/// <summary>
	/// Get all payout requests for staff dashboard
	/// </summary>
	[Authorize(Roles = RoleConstant.STAFF)]
	[HttpGet("requests")]
	public async Task<IActionResult> GetAllPayoutRequests([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
	{
		try
		{
			var result = await _payoutService.GetAllPayoutRequests(pageIndex, pageSize, status);
			if (result.StatusCode != StatusCodes.Status200OK)
				return StatusCode(result.StatusCode, result);

			return Ok(result);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return StatusCode(500, new { message = "Server error when getting payout requests: " + ex.Message });
		}
	}

	[Authorize(Roles = RoleConstant.STAFF)]
	[HttpPatch("update-status")]
	public async Task<IActionResult> UpdatePayoutStatus([FromQuery] Guid payoutId, PayoutRequestStatusEnum? status)
	{
		var result = await _payoutService.ChangePayoutStatus(payoutId, status??0);
		if (result.StatusCode != StatusCodes.Status200OK)
			return StatusCode(result.StatusCode, result);

		return Ok(result);
	}

	/// <summary>
	/// Staff starts processing a payout request (change status to BUSY)
	/// </summary>
	[Authorize(Roles = RoleConstant.STAFF)]
	[HttpPatch("{payoutId}/start-processing")]
	public async Task<IActionResult> StartProcessingPayout([FromRoute] Guid payoutId)
	{
		try
		{
			// Get staff user ID from JWT token
			var userIdClaim = Helper.UserClaims.GetUserIdFromJwtToken(HttpContext.User.Claims);
			if (string.IsNullOrEmpty(userIdClaim))
			{
				return BadRequest(new { message = "Unable to get staff user ID from token" });
			}

			var result = await _payoutService.StartProcessingPayout(payoutId, new Guid(userIdClaim));
			if (result.StatusCode != StatusCodes.Status200OK)
				return StatusCode(result.StatusCode, result);

			return Ok(result);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return StatusCode(500, new { message = "Server error when starting payout processing: " + ex.Message });
		}
	}

	/// <summary>
	/// Get payout requests for a specific keeper
	/// </summary>
	[Authorize(Roles = RoleConstant.KEEPER)]
	[HttpGet("my-requests")]
	public async Task<IActionResult> GetMyPayoutRequests([FromQuery] Guid keeperId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
	{
		try
		{
			var result = await _payoutService.GetPayoutRequestsByKeeper(keeperId, pageIndex, pageSize);
			if (result.StatusCode != StatusCodes.Status200OK)
				return StatusCode(result.StatusCode, result);

			return Ok(result);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return StatusCode(500, new { message = "Server error when getting keeper payout requests: " + ex.Message });
		}
	}

	/// <summary>
	/// Check payout status for an order
	/// </summary>
	[Authorize(Roles = RoleConstant.KEEPER)]
	[HttpGet("order-status/{orderId}")]
	public async Task<IActionResult> GetOrderPayoutStatus([FromRoute] Guid orderId, [FromQuery] Guid keeperId)
	{
		try
		{
			var result = await _payoutService.GetOrderPayoutStatus(orderId, keeperId);
			return Ok(result);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return StatusCode(500, new { message = "Server error when checking order payout status: " + ex.Message });
		}
	}

	/// <summary>
	/// Check if an order is eligible for payout request
	/// </summary>
	[Authorize(Roles = RoleConstant.KEEPER)]
	[HttpGet("check-eligibility/{orderId}")]
	public async Task<IActionResult> CheckPayoutEligibility([FromRoute] Guid orderId, [FromQuery] Guid keeperId)
	{
		try
		{
			var result = await _payoutService.CheckPayoutEligibility(orderId, keeperId);
			return Ok(result);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return StatusCode(500, new { message = "Server error when checking payout eligibility: " + ex.Message });
		}
	}

	[Authorize(Roles = RoleConstant.STAFF)]
	[HttpPut("complete-payout/{payoutId}")]
	public async Task<IActionResult> CompletePayout([FromBody] string description, Guid payoutId)
	{
		long longOrderCode = Helper.Utility.GenerateSecureOrderCode();
		var result = await _payoutService.CompletePayout(payoutId, longOrderCode.ToString(), description);
		if (result.StatusCode != StatusCodes.Status200OK)
			return StatusCode(result.StatusCode, result);

		return Ok(result);
	}
}
