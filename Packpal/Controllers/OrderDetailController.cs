using DataAccess.Constants;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packpal.BLL.Interface;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;
        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

		[Authorize]
		[HttpGet("order/{id:guid}")]
        public async Task<IActionResult> GetAllAsyncByOrderId(Guid id, int page = 1, int pageSize = 5)
        {
            BaseResponseModel response;
            if (id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid order detail ID."
                );
                return StatusCode(response.StatusCode, response);
            }
            var orders = await _orderDetailService.GetAllOrderDetailsByOrderIdAsync(id, page, pageSize);
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: orders,
                message: "Get order details success."
            );
            return StatusCode(response.StatusCode, response);
        }

		[Authorize]
		[HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var order = await _orderDetailService.GetOrderDetailByIdAsync(id);
            BaseResponseModel response;
            if (order == null)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "order detail not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: order,
                message: "Get order detail success."
            );
            return StatusCode(response.StatusCode, response);
        }

		[Authorize]
		[HttpPost("{orderId:guid}")]
        public async Task<IActionResult> CreateAsync([FromBody] List<CreateOrderDetailModel> model, Guid orderId)
        {
            BaseResponseModel response;
            if (model == null || !ModelState.IsValid ||orderId==Guid.Empty )
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Bad Request, please use input the order detail correctly and order ID."
                );
                return StatusCode(response.StatusCode, response);
            }
            string result = await _orderDetailService.CreateOrderDetailAsync(model,orderId);
            if (result.Equals("404"))
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Fail to create order detail due to Order not found"
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Create order detail success."
            );
            return StatusCode(response.StatusCode, response);
        }

		[Authorize]
		[HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateOrderDetailModel model)
        {
            BaseResponseModel response;
            if (model == null || model.Id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid update order detail model."
                );
                return StatusCode(response.StatusCode, response);
            }
            var result = await _orderDetailService.UpdateOrderDetailAsync(model);
            if (result == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Order detail not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: model.Id,
                message: "Update order detail success."
            );
            return StatusCode(response.StatusCode, response);
        }

		[Authorize]
		[HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            BaseResponseModel response;
            if (id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid order detail ID."
                );
                return StatusCode(response.StatusCode, response);
            }
            var result = await _orderDetailService.DeleteOrderDetailAsync(id);
            if (!result)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Order detail not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: id,
                message: "Delete order detail success."
            );
            return StatusCode(response.StatusCode, response);
        }
    }
}
