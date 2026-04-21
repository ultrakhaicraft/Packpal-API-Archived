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
    [Authorize]
    public class SizeController : ControllerBase
    {
        private readonly ISizeService _sizeService;
        public SizeController(ISizeService sizeService)
        {
            _sizeService = sizeService;
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync([FromQuery] SizeQuery query)
        {
            var sizes = await _sizeService.GetAllSizesAsync(query);
            var response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: sizes,
                message: "Get size success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var size = await _sizeService.GetSizeByIdAsync(id);
            BaseResponseModel response;
            if (size == null)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Size not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: size,
                message: "Get size by ID success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSizeModel model)
        {
            BaseResponseModel response;
            if (model == null)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Size not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            var id = await _sizeService.CreateSizeAsync(model);
            if (id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Fail to create size."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: id,
                message: "Create size model success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateSizeModel model)
        {
            BaseResponseModel response;
            if (model == null || model.Id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid update size model."
                );
                return StatusCode(response.StatusCode, response);
            }
            var result = await _sizeService.UpdateSizeAsync(model);
            if (result == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Size not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: model.Id,
                message: "Update size model success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            BaseResponseModel response;
            if (id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid size ID."
                );
                return StatusCode(response.StatusCode, response);
            }
            var result = await _sizeService.DeleteSizeAsync(id);
            if (!result)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Size not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: id,
                message: "Delete size success."
            );
            return StatusCode(response.StatusCode, response);
        }
    }
}
