using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packpal.BLL.Interface;
using Packpal.DAL.ModelViews.EntityModel;
using System.Linq;
using DataAccess.ResponseModel;

namespace Packpal.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	{
		_authService = authService;
	}

	[AllowAnonymous]
	[HttpPost("login")]
	public async Task<IActionResult> Login(LoginRequest model)
	{
		if (!ModelState.IsValid)
		{
			var errors = ModelState
				.Where(x => x.Value?.Errors.Count > 0)
				.ToDictionary(
					kvp => kvp.Key,
					kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? new string[0]
				);
			
			var validationResponse = new BaseResponseModel(
				statusCode: 400,
				code: "VALIDATION_ERROR",
				data: null,
				additionalData: errors,
				message: "Validation failed"
			);
			
			return BadRequest(validationResponse);
		}
		
		var response = await _authService.LoginAsync(model);

		if (response.StatusCode.Equals(StatusCodes.Status200OK))
		{
			HttpContext.Items["CustomMessage"] = response.Message;
			return StatusCode(response.StatusCode,response);
		}
		else
		{
			return StatusCode(response.StatusCode, response);
		}
	}

	[AllowAnonymous]
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] UserCreateRequest model)
	{
		if (!ModelState.IsValid)
		{
			var errors = ModelState
				.Where(x => x.Value?.Errors.Count > 0)
				.ToDictionary(
					kvp => kvp.Key,
					kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? new string[0]
				);
			
			var validationResponse = new BaseResponseModel(
				statusCode: 400,
				code: "VALIDATION_ERROR",
				data: null,
				additionalData: errors,
				message: "Validation failed"
			);
			
			return BadRequest(validationResponse);
		}
		
		var response = await _authService.RegisterAsync(model);

		if (response.StatusCode.Equals(StatusCodes.Status200OK))
		{
			// ✅ Return data object only, let middleware wrap it
			
			return Ok(response);
		}
		else
		{
			return StatusCode(response.StatusCode, response);
		}
	}

	[AllowAnonymous]
	[HttpPatch("change-password")]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model, string userEmail, bool IsForgot)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest();
		}
		var response = await _authService.ChangePasswordAsync(model, userEmail,IsForgot);

		if (response.StatusCode.Equals(StatusCodes.Status200OK))
		{
			// ✅ Return data object only, let middleware wrap it
			
			return Ok(response);
		}
		else
		{
			return StatusCode(response.StatusCode, response);
		}
	}
}
