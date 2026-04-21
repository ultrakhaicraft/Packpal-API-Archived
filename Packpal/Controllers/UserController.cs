using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packpal.BLL.Interface;
using Packpal.DAL.ModelViews.EntityModel;
using Packpal.Helper;
namespace Packpal.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
	private readonly IUserService _accountService;
	public UserController(IUserService userService)
	{
		_accountService = userService;
	}

	[HttpGet("get-all")]
	public async Task<IActionResult> GetAllAccount([FromQuery] UserQuery request)
	{
		var response = await _accountService.GetAllAccountsAsync(request);
		if(response.StatusCode== 200)
		{
			return Ok(response);
		}
		else
		{
			return StatusCode(response.StatusCode, response);
		}
	}
	[HttpGet("get-detail")]
	public async Task<IActionResult> GetAccountDetailByID([FromQuery] string userId)
	{
		var response = await _accountService.GetAccountDetailAsync(userId);
		if (response.StatusCode == 200)
		{
			return Ok(response);
		}
		else
		{
			return StatusCode(response.StatusCode, response);
		}
	}

	[HttpPost("create-account")]
	public async Task<IActionResult> CreateNewAccount([FromBody] UserCreateRequest request)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}
		var response = await _accountService.CreateAsync(request);
		if (response.StatusCode == 200)
		{
			return Ok(response);
		}
		else
		{
			return StatusCode(response.StatusCode, response);
		}
	}

	[HttpPut("update-account")]
	public async Task<IActionResult> UpdateAccount([FromBody] UserUpdateModel request, string userId)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}
		var response = await _accountService.UpdateAsync(request, userId);
		if (response.StatusCode == 200)
		{
			return Ok(response);
		}
		else
		{
			return StatusCode(response.StatusCode, response);
		}
	}

	[HttpDelete("delete-account")]
	public async Task<IActionResult> SoftDeleteAccount([FromQuery] string userId)
	{
		var response = await _accountService.DeleteAsync(userId);
		if (response.StatusCode == 200)
		{
			return Ok(response);
		}
		else
		{
			return StatusCode(response.StatusCode, response);
		}
	}

	[HttpPost("register-keeper")]
	public async Task<IActionResult> RegisterKeeper([FromForm] KeeperRegisterForm request)
	{
		if (!ModelState.IsValid)
		{
			Console.WriteLine("Something wrong with model State: ");
			return BadRequest(ModelState);
		}
		if (request.Documents == null || request.Documents.Length == 0)
			return BadRequest("No file selected.");

		if (!Utility.IsPdfFile(request.Documents))
			return BadRequest("Invalid file format. Only PDF files are allowed.");

		//Impose a file size limit (e.g., 10 MB)
		if (request.Documents.Length > 10 * 1024 * 1024)
			return BadRequest("File size exceeds the limit of 10 MB.");

		var result = await _accountService.RegisterKeeper(request, request.Documents);
		if (result.StatusCode != StatusCodes.Status200OK)
		{
			return StatusCode(result.StatusCode, result);
		}
		return Ok(result);
	}

	[HttpPost("register-keeper-from-request")]
	public async Task<IActionResult> RegisterKeeperFromRequest([FromBody] RegisterKeeperFromRequestModel request)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		// Convert to KeeperRegistrationData
		var keeperData = new KeeperRegistrationData
		{
			Email = request.Email,
			IdentityNumber = request.IdentityNumber,
			BankAccount = request.BankAccount,
			DocumentsUrl = request.DocumentsUrl
		};

		var result = await _accountService.RegisterKeeperFromRequestAsync(request.UserId, keeperData);
		if (result.StatusCode != StatusCodes.Status200OK)
		{
			return StatusCode(result.StatusCode, result);
		}
		return Ok(result);
	}

    [HttpDelete("ban-account")]
    public async Task<IActionResult> BanDeleteAccount([FromQuery] string userId)
    {
        var result = await _accountService.BanAsync(userId);
        HttpContext.Items["CustomMessage"] = "Account banned successfully!";
        return Ok(result.Data);
    }
	[HttpPatch("update-avatar")] 
	public async Task<IActionResult> UpdateUserAvatar([FromForm] Guid id, IFormFile image)
	{
		if (!Utility.IsImageFile(image))
		{
			var response = BaseResponseModel<string>.BadRequestResponseModel("", "Invalid file format. Only Image files are allowed.");
			return BadRequest(response);
		}
			
		if (image.Length > 10 * 1024 * 1024)
			return BadRequest("File size exceeds the limit of 10 MB.");

		var result = await _accountService.UpdateAvatarUrl(id, image);
		if (result.StatusCode != StatusCodes.Status200OK)
		{
			return StatusCode(result.StatusCode, result);
		}
		return Ok(result);
	}

	[HttpPost("switch-role")]
	public async Task<IActionResult> SwitchRole([FromQuery] string userEmail, [FromBody] SwitchRoleRequest request)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var response = await _accountService.SwitchRole(userEmail, request);
		if (response.StatusCode == 200)
		{
			return Ok(response);
		}
		else
		{
			return StatusCode(response.StatusCode, response);
		}
	}
}
