using DataAccess.Constants;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packpal.BLL.Interface;
using Packpal.DAL.Enum;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : Controller
{
	private readonly IOrderService _orderService;
	public OrderController(IOrderService orderService)
	{
		_orderService = orderService;
	}

    /// <summary>
    /// Get all orders from a storage by storage ID
    /// </summary>
    /// <param name="id">Storage ID</param>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("storage/{id:guid}")]
    public async Task<IActionResult> GetAllByStorageIdAsync(Guid id,[FromQuery] OrderQuery query)
    {
        BaseResponseModel response;
        if (ModelState.IsValid == false || id == Guid.Empty)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid request parameters."
            );
            return StatusCode(response.StatusCode, response);
        }
        var orders = await _orderService.GetAllOrderFromAStorageAsync(query, id);
        if (orders == null)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "List of Orders not found."
            );
            return StatusCode(response.StatusCode, response);
        }
        HttpContext.Items["CustomMessage"] = "Get all orders success.";
        return Ok(orders);
    }

	/// <summary>
	/// Get all orders from a user by user ID
	/// OrderStatus: {0 = PENDING, 1 = CONFIRMED, 2 = IN_STORAGE, 3 = COMPLETED, 4 = CANCELLED}
	/// </summary>
	/// <param name="id">User Id</param>
	/// <param name="query"></param>
	/// <returns></returns>
	[Authorize]
	[HttpGet("user/{id:guid}")]
	public async Task<IActionResult> GetAllByUserIdAsync(Guid id, [FromQuery] OrderQuery query)
	{
		BaseResponseModel response;
		if (ModelState.IsValid == false || id == Guid.Empty)
		{
			response = new BaseResponseModel(
				statusCode: StatusCodes.Status400BadRequest,
				code: ResponseCodeConstants.BAD_REQUEST,
				message: "Invalid request parameters."
			);
			return StatusCode(response.StatusCode, response);
		}
		var orders = await _orderService.GetAllOrderFromAUserAsync(query, id);
		if (orders == null)
		{
			response = new BaseResponseModel(
				statusCode: StatusCodes.Status404NotFound,
				code: ResponseCodeConstants.NOT_FOUND,
				message: "List of Orders not found."
			);
			return StatusCode(response.StatusCode, response);
		}
		// ✅ Return data object only, let middleware wrap it
		HttpContext.Items["CustomMessage"] = "Get all orders success.";
		return Ok(orders);
	}

	/// <summary>
	/// Get all orders from a keeper by keeper ID
	/// OrderStatus: {0 = PENDING, 1 = CONFIRMED, 2 = IN_STORAGE, 3 = COMPLETED, 4 = CANCELLED}
	/// </summary>
	/// <param name="id">Keeper Id</param>
	/// <param name="query"></param>
	/// <returns></returns>
	[Authorize]
	[HttpGet("keeper/{id:guid}")]
	public async Task<IActionResult> GetAllByKeeperIdAsync(Guid id, [FromQuery] OrderQuery query)
	{
		BaseResponseModel response;
		if (ModelState.IsValid == false || id == Guid.Empty)
		{
			response = new BaseResponseModel(
				statusCode: StatusCodes.Status400BadRequest,
				code: ResponseCodeConstants.BAD_REQUEST,
				message: "Invalid request parameters."
			);
			return StatusCode(response.StatusCode, response);
		}
		var orders = await _orderService.GetAllOrderFromAKeeperAsync(query, id);
		if (orders == null)
		{
			response = new BaseResponseModel(
				statusCode: StatusCodes.Status404NotFound,
				code: ResponseCodeConstants.NOT_FOUND,
				message: "List of Orders not found."
			);
			return StatusCode(response.StatusCode, response);
		}
		response = new BaseResponseModel(
			statusCode: StatusCodes.Status200OK,
			code: ResponseCodeConstants.SUCCESS,
			data: orders,
			message: "Get all keeper orders success."
		);
		return StatusCode(response.StatusCode, response);
	}

    /// <summary>
    /// Get order by ID of Order
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        BaseResponseModel response;
        if (ModelState.IsValid == false || id == Guid.Empty)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid request parameters."
            );
            return StatusCode(response.StatusCode, response);
        }
        var order = await _orderService.GetOrderDetailByIdAsync(id);
        if (order == null || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "Order not found."
            );
            return StatusCode(response.StatusCode, response);
        }
        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            data: order,
            message: "Get order by ID success."
        );
        return StatusCode(response.StatusCode, response);
    }
	[Authorize] // Temporarily removed role restriction for testing
	[HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateOrderModel model)
    {
        BaseResponseModel response;
        if (model == null || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Request Body is either empty or invalid"
            );
            return StatusCode(response.StatusCode, response);
        }
        var id = await _orderService.CreateOrderAsync(model);
        if (id == Guid.Empty)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Fail to create Order."
            );
            return StatusCode(response.StatusCode, response);
        }
        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            data: id,
            message: "Create Order successful"
        );
        return StatusCode(response.StatusCode, response);
    }
	[Authorize]
	[HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateOrderModel model)
    {
        BaseResponseModel response;
        if (model == null || model.Id == Guid.Empty || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid update Order model."
            );
            return StatusCode(response.StatusCode, response);
        }
        var result = await _orderService.UpdateOrderAsync(model);
        if (result == Guid.Empty)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "Order might not found."
            );
            return StatusCode(response.StatusCode, response);
        }
        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            data: model.Id,
            message: "Update Order successful"
        );
        return StatusCode(response.StatusCode, response);
    }

	/// <summary>
	/// Patch update order - supports partial field updates
	/// </summary>
	/// <param name="model">Order patch model with optional fields</param>
	/// <returns></returns>
	[Authorize] // Temporarily removed role restriction for testing
	[HttpPatch]
    public async Task<IActionResult> PatchUpdateAsync([FromBody] PatchOrderModel model)
    {
        BaseResponseModel response;
        if (model == null || model.Id == Guid.Empty || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid patch order model."
            );
            return StatusCode(response.StatusCode, response);
        }
        
        var result = await _orderService.PatchUpdateOrderAsync(model);
        if (result == Guid.Empty)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "Order not found or update failed."
            );
            return StatusCode(response.StatusCode, response);
        }
        
        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            data: model.Id,
            message: "Order updated successfully"
        );
        return StatusCode(response.StatusCode, response);
    }

	[Authorize]
	[HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        BaseResponseModel response;
        if (id == Guid.Empty || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid Order ID."
            );
            return StatusCode(response.StatusCode, response);
        }
        var result = await _orderService.DeleteOrderAsync(id);
        if (!result)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "Order not found."
            );
            return StatusCode(response.StatusCode, response);
        }
        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            data: id,
            message: "Delete Order success."
        );
        return StatusCode(response.StatusCode, response);
    }

	[Authorize] // Allow both KEEPER and RENTER for payment update
	[HttpPut("paid/{id:guid}")]
    public async Task<IActionResult> UpdateIsPaidAsync(Guid id)
    {
        BaseResponseModel response;
        if (id == Guid.Empty || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid order id."
            );
            return StatusCode(response.StatusCode, response);
        }
        var result = await _orderService.UpdateIsPaidAsync(id);
        if (!result)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "Order not found."
            );
            return StatusCode(response.StatusCode, response);
        }
        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            message: "Update isPaid status success."
        );
        return StatusCode(response.StatusCode, response);
    }
	
    
    [Authorize]
	[HttpPut("status/{id:guid}")]
    public async Task<IActionResult> UpdateStatusAsync(Guid id, [FromBody] string status)
    {
        BaseResponseModel response;
        if (id == Guid.Empty || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid request."
            );
            return StatusCode(response.StatusCode, response);
        }

        var isStatusValid = Enum.TryParse<OrderStatusEnum>(status, true, out var parsedStatus);
        if (!isStatusValid)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid status."
            );
            return StatusCode(response.StatusCode, response);
        }
        
        var result = await _orderService.UpdateOrderStatusAsync(id, status);
        if (!result)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "Order not found."
            );
            return StatusCode(response.StatusCode, response);
        }
        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            message: "Update status success."
        );
        return StatusCode(response.StatusCode, response);
    }

	[Authorize]
	[HttpPut("start-time/{id:guid}")]
    public async Task<IActionResult> UpdateStartTimeAsync(Guid id)
    {
        BaseResponseModel response;
        if (id == Guid.Empty || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid request."
            );
            return StatusCode(response.StatusCode, response);
        }

        var result = await _orderService.StartKeepTimeAsync(id);
        if (!result)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "Order not found."
            );
            return StatusCode(response.StatusCode, response);
        }
        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            message: "Start the time success."
        );
        return StatusCode(response.StatusCode, response);
    }


	/// <summary>
	/// Upload certification images for an order
	/// </summary>
	/// <param name="id">Order ID</param>
	/// <param name="imageUrls">Array of image URLs (max 2)</param>
	/// <returns></returns>
	[Authorize]
	[HttpPost("{id:guid}/certification")]
    public async Task<IActionResult> UpdateOrderCertificationAsync(Guid id, [FromBody] string[] imageUrls)
				{
        BaseResponseModel response;
        if (id == Guid.Empty || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid request."
            );
            return StatusCode(response.StatusCode, response);
        }
				if (imageUrls == null || imageUrls.Length == 0 || imageUrls.Length > 2)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Please provide 1-2 certification images."
            );
            return StatusCode(response.StatusCode, response);
        }

        var result = await _orderService.UpdateOrderCertificationAsync(id, imageUrls);
        if (!result)
				{
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
								message: "Order not found."
            );
            return StatusCode(response.StatusCode, response);
        }

        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            message: "Certification images uploaded successfully."
        );
        return StatusCode(response.StatusCode, response);
    }

	/// <summary>
	/// Calculate final amount with overtime fees for pickup
	/// </summary>
	/// <param name="id">Order ID</param>
	/// <returns></returns>
	[Authorize]
	[HttpGet("{id:guid}/calculate-fee")]
    public async Task<IActionResult> CalculateFinalAmountAsync(Guid id)
    {
        BaseResponseModel response;
        if (id == Guid.Empty)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid order ID."
            );
            return StatusCode(response.StatusCode, response);
        }

        try
        {
            var finalAmount = await _orderService.CalculateFinalAmountAsync(id);
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Final amount calculated successfully.",
                data: new { FinalAmount = finalAmount }
            );
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status500InternalServerError,
                code: ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                message: ex.Message
            );
            return StatusCode(response.StatusCode, response);
        }
    }

    /// <summary>
    /// Get Total Amount of the Order
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
	[Authorize]
	[HttpGet("total-amount/{id:guid}")]
    public async Task<IActionResult> GetTotalAmountAsync(Guid id)
    {
        BaseResponseModel response;
        if (id == Guid.Empty || ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid request."
            );
            return StatusCode(response.StatusCode, response);
        }
        var totalAmount = await _orderService.GetTotalAmountAsync(id);
        if (totalAmount < 0)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "Order not found or order has ever in the storage."
            );
            return StatusCode(response.StatusCode, response);
        }
        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            data: totalAmount,
            message: "Get total amount success."
        );
        return StatusCode(response.StatusCode, response);
    }

	/// <summary>
	/// Get countdown information for an order including time remaining, expiration status, and percentage complete
	/// </summary>
	/// <param name="id">Order ID</param>
	/// <returns>OrderCountdownModel with detailed countdown information</returns>

	[Authorize]
	[HttpGet("{id:guid}/countdown")]
    public async Task<IActionResult> GetOrderCountdownAsync(Guid id)
    {
        BaseResponseModel response;
        if (ModelState.IsValid == false || id == Guid.Empty)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid order ID provided."
            );
            return StatusCode(response.StatusCode, response);
        }

        var countdown = await _orderService.GetOrderCountdownAsync(id);
        if (countdown == null)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status404NotFound,
                code: ResponseCodeConstants.NOT_FOUND,
                message: "Order not found or storage period has not started yet."
            );
            return StatusCode(response.StatusCode, response);
        }

        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            data: countdown,
            message: "Get order countdown success."
        );
        return StatusCode(response.StatusCode, response);
    }

	/// <summary>
	/// Get countdown information for multiple orders (bulk operation for better performance)
	/// </summary>
	/// <param name="orderIds">List of Order IDs</param>
	/// <returns>List of OrderCountdownModel with detailed countdown information</returns>
	[Authorize]
	[HttpPost("countdown/bulk")]
    public async Task<IActionResult> GetMultipleOrderCountdownAsync([FromBody] List<Guid> orderIds)
    {
        BaseResponseModel response;
        if (ModelState.IsValid == false || orderIds == null || !orderIds.Any())
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid or empty order IDs list provided."
            );
            return StatusCode(response.StatusCode, response);
        }

        // Limit to maximum 50 orders per request to prevent abuse
        if (orderIds.Count > 50)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Maximum 50 orders allowed per request."
            );
            return StatusCode(response.StatusCode, response);
        }

        var countdowns = await _orderService.GetMultipleOrderCountdownAsync(orderIds);

        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            data: countdowns,
            message: $"Get countdown for {countdowns.Count} orders success."
        );
        return StatusCode(response.StatusCode, response);
    }
	/// <summary>
	/// Get all orders with pagination and filtering options
	/// OrderStatus: {0 = PENDING, 1 = CONFIRMED, 2 = IN_STORAGE, 3 = COMPLETED, 4 = CANCELLED}
    /// Get 
	/// </summary>
	/// <returns>List of OrderCountdownModel with detailed countdown information</returns>
	[Authorize]
	[HttpGet("all")]
    public async Task<IActionResult> GetAllAsync([FromQuery] OrderQuery query)
    {
        BaseResponseModel response;
        if (ModelState.IsValid == false)
        {
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status400BadRequest,
                code: ResponseCodeConstants.BAD_REQUEST,
                message: "Invalid request parameters."
            );
            return StatusCode(response.StatusCode, response);
        }
        var orders = await _orderService.GetAllOrderAsync(query);
        double totalAmount = 0;
        foreach(var order in orders.Data)
        {
            if(order.Status == OrderStatusEnum.COMPLETED.ToString())
            totalAmount += order.TotalAmount;
        }

        response = new BaseResponseModel(
            statusCode: StatusCodes.Status200OK,
            code: ResponseCodeConstants.SUCCESS,
            data: orders,
            additionalData: new 
            {
                TotalAmount = totalAmount,
                PlatformIncome = totalAmount * 0.2
            },
            message: "Search orders success"
        );
        return StatusCode(response.StatusCode, response);
    }
}
