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
    [Authorize]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync([FromQuery] RatingQuery query)
        {
            var ratings = await _ratingService.GetAllRatingsAsync(query);
            var response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: ratings,
                message: "Get rating success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            
            BaseResponseModel response;
            if (id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid rating ID."
                );
                return StatusCode(response.StatusCode, response);
            }
            var rating = await _ratingService.GetRatingByIdAsync(id);
            if (rating == null)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "rating not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: rating,
                message: "Get rating success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateRatingModel model)
        {
            BaseResponseModel response;
            if (model == null)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "rating not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            var id = await _ratingService.CreateRatingAsync(model);
            if (id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Fail to create rating."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: id,
                message: "Get rating success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateRatingModel model)
        {
            BaseResponseModel response;
            if (model == null || model.Id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid update rating model."
                );
                return StatusCode(response.StatusCode, response);
            }
            var result = await _ratingService.UpdateRatingAsync(model);
            if (result == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "rating not found."
                );
                return StatusCode(response.StatusCode, response);
            }

            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: model.Id,
                message: "Get rating success."
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
                    message: "Invalid rating ID."
                );
                return StatusCode(response.StatusCode, response);
            }
            var result = await _ratingService.DeleteRatingAsync(id);
            if (!result)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "rating not found."
                );
                return StatusCode(response.StatusCode, response);
            }
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: id,
                message: "Delete rating success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("renter/{id:guid}")]
        public async Task<IActionResult> GetAsyncByRenterId(Guid id, [FromQuery] RatingQuery query)
        {
            BaseResponseModel response;
            if (id == Guid.Empty)
            {
                 response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid renter ID."
                );
                return StatusCode(response.StatusCode, response);
            }
            var ratings = await _ratingService.GetRatingsByRenterIdAsync(id, query);
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: ratings,
                message: "Get rating success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("storage/{id:guid}")]
        public async Task<IActionResult> GetAsyncByStorageId(Guid id, [FromQuery] RatingQuery query)
        {
            BaseResponseModel response;
            if (id == Guid.Empty)
            {
                response = new BaseResponseModel(
                   statusCode: StatusCodes.Status400BadRequest,
                   code: ResponseCodeConstants.BAD_REQUEST,
                   message: "Invalid renter ID."
               );
                return StatusCode(response.StatusCode, response);
            }
            var ratings = await _ratingService.GetRatingsByStorageIdAsync(id, query);
            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: ratings,
                message: "Get rating success."
            );
            return StatusCode(response.StatusCode, response);
        }
    }
}
