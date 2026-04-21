using DataAccess.Constants;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packpal.BLL.Interface;
using Packpal.DAL.Constants;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _storageService;

        public StorageController(IStorageService storageService)
        {
            _storageService = storageService;
        }

		[Authorize]
		[HttpGet("all")]
        public async Task<IActionResult> GetAllAsync([FromQuery] StorageQuery query)
        {
            var storages = await _storageService.GetAllAsync(query);
            var response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: storages,
                message: "Get storage success."
            );
            return StatusCode(response.StatusCode, response);
        }

		[Authorize]
		[HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            BaseResponseModel response;
            if (id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid id."
                );
                return StatusCode(response.StatusCode, response);
            }
            var storage = await _storageService.GetByIdAsync(id);
            if (storage == null)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Storage not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: storage,
                message: "Get storage success."
            );
            return StatusCode(response.StatusCode, response);
        }

		[Authorize]
		[HttpGet("keepers/{keeperId:guid}/storages")]
        public async Task<IActionResult> GetStoragesByKeeperAsync(Guid keeperId)
        {
            BaseResponseModel response;
            if (keeperId == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid keeper id."
                );
                return StatusCode(response.StatusCode, response);
            }

            var storages = await _storageService.GetByKeeperIdAsync(keeperId);
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: storages,
                message: "Get keeper storages success."
            );
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAsync([FromBody] CreateStorageModel model)
        {
            BaseResponseModel response;
            if (model == null || !ModelState.IsValid)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid storage create model."
                );
                return StatusCode(response.StatusCode, response);
            }
            var id = await _storageService.CreateAsync(model);
            if (id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Fail to create storage."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: id,
                message: "Create storage success."
            );
            return StatusCode(response.StatusCode, response);
        }

	    [Authorize]
	    [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateStorageRequestModel requestModel)
        {
            BaseResponseModel response;
            if (requestModel == null || id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid update storage model."
                );
                return StatusCode(response.StatusCode, response);
            }

            // Convert request model to service model
            var model = new UpdateStorageModel
            {
                Id = id,
                Status = requestModel.Status,
                Description = requestModel.Description,
                Address = requestModel.Address,
                Latitude = requestModel.Latitude,
                Longitude = requestModel.Longitude
            };

            var result = await _storageService.UpdateAsync(model);
            if (!result)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Storage not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: id,
                message: "Update storage success."
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
                    message: "Invalid storage ID."
                );
                return StatusCode(response.StatusCode, response);
            }
            var result = await _storageService.DeleteAsync(id);
            if (!result)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Storage not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: id,
                message: "Delete storage success."
            );
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("distance")]
        public async Task<IActionResult> CalculateDistanceAsync([FromQuery] double lat1, [FromQuery] double lon1, [FromQuery] double lat2, [FromQuery] double lon2)
        {
            var distance = await _storageService.CalculateDistanceAsync(lat1, lon1, lat2, lon2);
            var response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: distance,
                message: "Calculate distance success."
            );
            return StatusCode(response.StatusCode, response);
        }

		[Authorize]
		[HttpGet("keepers/{keeperId:guid}/pending-orders-count")]
        public async Task<IActionResult> GetTotalPendingOrdersByKeeperIdAsync(Guid keeperId)
        {
            BaseResponseModel response;
            if (keeperId == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid keeper id."
                );
                return StatusCode(response.StatusCode, response);
            }

            var pendingOrdersCount = await _storageService.GetTotalPendingOrdersByKeeperIdAsync(keeperId);
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: pendingOrdersCount,
                message: "Get keeper pending orders count success."
                                );
            return StatusCode(response.StatusCode, response);
        }


        /*[HttpGet("keeper/{keeperId:guid}")]
        public async Task<IActionResult> GetAllByKeeperIdAsync([FromQuery] StorageQuery query, Guid keeperId)
        {
            var storages = await _storageService.GetAllByKeeperId(query, keeperId);
            var response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: storages,
                message: "Get storages success."
            );
            return StatusCode(response.StatusCode, response);
        }*/
    }
}


