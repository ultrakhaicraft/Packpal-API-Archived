using DataAccess.Constants;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packpal.BLL.Interface;
using Packpal.DAL.Enum;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IUserService _userService;

        public RequestController(IRequestService requestService, IUserService userService)
        {
            _requestService = requestService;
            _userService = userService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateRequestModel model)
        {
            BaseResponseModel response;

            if (model == null 
                || !ModelState.IsValid
                || !Enum.IsDefined(typeof(RequestTypeEnum), model.Type.ToUpper()))
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Invalid request create model."
                );
                return StatusCode(response.StatusCode, response);
            }

            var viewModel = await _requestService.CreateAsync(model);

            if (viewModel == null || viewModel.Id == Guid.Empty)
            {
                response = new BaseResponseModel(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    message: "Fail to create request."
                );
                return StatusCode(response.StatusCode, response);
            }

            response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: viewModel,
                message: "Create request success."
            );
            return StatusCode(response.StatusCode, response);
        }
        /// <summary>
        /// Status: PENDING = 1, APPROVED = 2, REJECTED = 3 || Type: KEEPER_REGISTRATION = 1, CREATESTORAGE = 2, DELETESTORAGE = 3
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] RequestQuery query)
        {
            var data = await _requestService.GetAllAsync(query);
            var response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: data,
                message: "Get all requests success."
            );
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var data = await _requestService.GetByIdAsync(id);
            if (data == null)
            {
                var notFoundResponse = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Request not found."
                );
                return StatusCode(notFoundResponse.StatusCode, notFoundResponse);
            }

            var response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: data,
                message: "Get request success."
            );
            return StatusCode(response.StatusCode, response);
        }
        /// <summary>
        /// PENDING = 1, APPROVED = 2, REJECTED = 3 
        /// </summary>
        /// <param name="requestId">request id to find the Request to update status</param>
        /// <param name="userId">User id of a staff account </param>
        /// <param name="status">The status to change into</param>
        /// <returns>ViewRequestModel</returns>
        [HttpPut("changeStatus")]
        public async Task<IActionResult> ChangeStatusAsync([FromQuery] Guid requestId, [FromQuery] Guid userId, [FromBody] RequestStatusEnum status)
        {
            var updatedRequest = await _requestService.UpdateStatusAsync(requestId, userId, status);
            if (updatedRequest == null)
            {
                var notFoundResponse = new BaseResponseModel(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    message: "Request not found."
                );
                return StatusCode(notFoundResponse.StatusCode, notFoundResponse);
            }

            var response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: updatedRequest,
                message: "Change request status success."
            );
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserIdAsync(Guid userId)
        {
            var data = await _requestService.GetByUserIdAsync(userId);
            var response = new BaseResponseModel(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: data,
                message: "Get user requests success."
            );
            return StatusCode(response.StatusCode, response);
        }
    }
}
