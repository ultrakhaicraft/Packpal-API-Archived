using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Packpal.BLL.Interface;
using Packpal.DAL.Enum;
using Packpal.DAL.ModelViews.EntityModel;
using DataAccess.Constants;

namespace Packpal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KeeperRegistrationController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public KeeperRegistrationController(
            IRequestService requestService, 
            IUserService userService,
            INotificationService notificationService)
        {
            _requestService = requestService;
            _userService = userService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Create a new keeper registration request (called from mobile app)
        /// </summary>
        [HttpPost("request")]
        [Authorize]
        public async Task<IActionResult> CreateKeeperRegistrationRequest([FromBody] CreateKeeperRegistrationRequestModel model)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Create keeper registration data object
                var keeperData = new KeeperRegistrationData
                {
                    Email = model.Email,
                    IdentityNumber = model.IdentityNumber,
                    BankAccount = model.BankAccount,
                    DocumentsUrl = model.DocumentsUrl // File đã được upload trước đó
                };

                // Serialize to JSON for storage in Request.Data
                string jsonData = JsonConvert.SerializeObject(keeperData);

                // Create request
                var createRequestModel = new CreateRequestModel
                {
                    UserId = model.UserId,
                    Type = "KEEPER_REGISTRATION",
                    Data = jsonData
                };

                var request = await _requestService.CreateAsync(createRequestModel);

                var response = new KeeperRegistrationRequestResponse
                {
                    RequestId = request.Id,
                    Status = request.Status,
                    Message = "Your keeper registration request has been submitted and is pending approval.",
                    RequestedAt = request.RequestedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Approve keeper registration request (called by staff)
        /// </summary>
        [HttpPost("{requestId}/approve")]
        [Authorize]
        public async Task<IActionResult> ApproveKeeperRegistration(Guid requestId, [FromBody] Guid staffUserId)
        {
            try
            {
                // Get the request
                var request = await _requestService.GetByIdAsync(requestId);
                if (request == null)
                {
                    return NotFound(new { message = "Request not found." });
                }

                if (request.Type != "KEEPER_REGISTRATION")
                {
                    return BadRequest(new { message = "Invalid request type." });
                }

                if (request.Status != RequestStatusEnum.PENDING.ToString())
                {
                    return BadRequest(new { message = "Request is not in pending status." });
                }

                // Parse keeper registration data from JSON
                if (string.IsNullOrEmpty(request.Data))
                {
                    return BadRequest(new { message = "Request data is missing." });
                }

                var keeperData = JsonConvert.DeserializeObject<KeeperRegistrationData>(request.Data);
                if (keeperData == null)
                {
                    return BadRequest(new { message = "Invalid request data format." });
                }

                // Call user service to register keeper with data from request
                var result = await _userService.RegisterKeeperFromRequestAsync(request.UserId, keeperData);
                if (result.StatusCode != StatusCodes.Status200OK || result.Code != ResponseCodeConstants.SUCCESS)
                {
                    return BadRequest(new { message = result.Message });
                }

                // Update request status to approved
                await _requestService.UpdateStatusAsync(requestId, staffUserId, RequestStatusEnum.APPROVED);

                // Send notification to user about approval via SignalR
                await _notificationService.NotifyUserKeeperRegistrationApprovedAsync(
                    request.UserId,
                    "Congratulations! Your keeper registration has been approved. You can now access keeper features."
                );

                return Ok(new { 
                    message = "Keeper registration approved successfully.",
                    requestId = requestId,
                    userId = request.UserId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while approving the request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Reject keeper registration request (called by staff)
        /// </summary>
        [HttpPost("{requestId}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectKeeperRegistration(Guid requestId, [FromBody] RejectRequestModel model)
        {
            try
            {
                // Get the request
                var request = await _requestService.GetByIdAsync(requestId);
                if (request == null)
                {
                    return NotFound(new { message = "Request not found." });
                }

                if (request.Type != "KEEPER_REGISTRATION")
                {
                    return BadRequest(new { message = "Invalid request type." });
                }

                if (request.Status != RequestStatusEnum.PENDING.ToString())
                {
                    return BadRequest(new { message = "Request is not in pending status." });
                }

                // Update request status to rejected
                await _requestService.UpdateStatusAsync(requestId, model.StaffUserId, RequestStatusEnum.REJECTED);

                // Send notification to user about rejection via SignalR
                var rejectionMessage = !string.IsNullOrEmpty(model.Reason) 
                    ? $"Your keeper registration request has been rejected. Reason: {model.Reason}"
                    : "Your keeper registration request has been rejected. Please contact support for more information.";
                    
                await _notificationService.NotifyUserKeeperRegistrationRejectedAsync(
                    request.UserId,
                    rejectionMessage
                );

                return Ok(new { 
                    message = "Keeper registration rejected successfully.",
                    requestId = requestId,
                    userId = request.UserId,
                    reason = model.Reason
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while rejecting the request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get keeper registration requests for staff review
        /// </summary>
        [HttpGet("pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingKeeperRegistrations([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new RequestQuery
                {
                    Type = RequestTypeEnum.KEEPER_REGISTRATION,
                    Status = RequestStatusEnum.PENDING,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                var requests = await _requestService.GetAllAsync(query);
                
                // Parse Data field for each request to include keeper details
                var enhancedRequests = requests.Data?.Select(r => new
                {
                    r.Id,
                    r.UserId,
                    r.Username,
                    r.Type,
                    r.Status,
                    r.RequestedAt,
                    r.ReviewedAt,
                    r.ReviewedBy,
                    KeeperData = !string.IsNullOrEmpty(r.Data) 
                        ? JsonConvert.DeserializeObject<KeeperRegistrationData>(r.Data)
                        : null
                }).ToList();

                return Ok(new
                {
                    Data = enhancedRequests,
                    requests.TotalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving pending requests.", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Model for rejecting a request
    /// </summary>
    public class RejectRequestModel
    {
        public Guid StaffUserId { get; set; }
        public string? Reason { get; set; }
    }
}
