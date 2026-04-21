using DataAccess.ResponseModel;
using Microsoft.Extensions.Configuration;
using Packpal.BLL.Interface;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;

using Packpal.DAL.Entity;

using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;
using System.Security.Claims;
using System.Text.RegularExpressions;


namespace Packpal.BLL.Services;

public class AuthService : IAuthService
{

	private readonly IUnitOfWork _unitOfWork;
	private readonly IJwtUtils _jwtUtils;
	private readonly IConfiguration _configuration;
	public AuthService(IUnitOfWork unitOfWork, IJwtUtils jwtUtils, IConfiguration configuration)
	{
		_unitOfWork = unitOfWork;
		_jwtUtils = jwtUtils;
		_configuration = configuration;
	}


	public async Task<BaseResponseModel<JWTToken>> LoginAsync(LoginRequest model)
	{
		try
		{
			Console.WriteLine($"🔍 LoginAsync started for email: {model.Email}");
			
			// Test direct database access first
			var userRepository = _unitOfWork.GetRepository<User>();
			Console.WriteLine($"🔍 Repository obtained: {userRepository?.GetType().Name}");
			
			// Try async version first
			Console.WriteLine($"🔍 Attempting FindAsync...");
			var user = await userRepository?.FindAsync(user => user.Email == model.Email)!;
			Console.WriteLine($"🔍 FindAsync result: {(user != null ? $"Found user {user.Email}" : "User not found")}");

			if (user == null)
			{
				Console.WriteLine($"❌ User not found for email: {model.Email}");
				return BaseResponseModel<JWTToken>.NotFoundResponseModel(null, message: "Email không tồn tại trong hệ thống");
			}

			Console.WriteLine($"🔍 Verifying password...");
			if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
			{
				Console.WriteLine($"❌ Password verification failed for user: {model.Email}");
				return BaseResponseModel<JWTToken>.NotFoundResponseModel(null, message: "Email hoặc mật khẩu không chính xác");
			}
			
			Console.WriteLine($"✅ Password verified, generating token...");
			var authClaims = new List<Claim>
			{
				new Claim("id", user.Id.ToString()),
				new Claim(ClaimTypes.Email, user?.Email ?? ""),
				new Claim(ClaimTypes.Role, user?.ActiveRole ?? ""),
			};
			await Task.Delay(100); // Simulate async operation
			var token = _jwtUtils.GenerateToken(authClaims, _configuration.GetSection("JwtSettings").Get<JwtModel>(), user);

			return BaseResponseModel<JWTToken>.OkResponseModel(token, message: "Login successful"); 
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<JWTToken>.InternalErrorResponseModel(null, message: "Server caught an error " + e.Message);
		}
	}

	public async Task<BaseResponseModel<string>> RegisterAsync(UserCreateRequest request)
	{
		try
		{
			_unitOfWork.BeginTransaction();
			//Validation Area
			var existingUser = _unitOfWork.GetRepository<User>().Find(x => x.Email == request.Email);

			if (existingUser != null)
				return BaseResponseModel<string>.BadRequestResponseModel(string.Empty, message: ErrorMessage.AccountExist);

			if (!IsValid(request.Password))
				return BaseResponseModel<string>.BadRequestResponseModel(string.Empty, message: ErrorMessage.ValidatePassword);

			if (request.Password != request.ConfirmPassword)
				return BaseResponseModel<string>.BadRequestResponseModel(string.Empty, message: ErrorMessage.ConfirmPasswordNotMatch);

			if (!IsValidPhoneNumber(request.PhoneNumber))
				return BaseResponseModel<string>.BadRequestResponseModel(string.Empty, message: ErrorMessage.InvalidPhoneNumber);


			//Create User
			var user = new User
			{
				Username = request.Username,
				Email = request.Email,
				PhoneNumber = request.PhoneNumber,
				Password = BCrypt.Net.BCrypt.HashPassword(request.Password),

			};
			await _unitOfWork.GetRepository<User>().InsertAsync(user);

			//Create Renter
			var renter = new Renter
			{
				UserId = user.Id,
			};

			
			await _unitOfWork.GetRepository<Renter>().InsertAsync(renter);
			await _unitOfWork.SaveAsync();
			_unitOfWork.CommitTransaction();

			
			return BaseResponseModel<string>.OkResponseModel(user.Id.ToString(), message: "Register as Renter successful");
		}
		catch (Exception e)
		{
			_unitOfWork.RollBack();
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<string>.InternalErrorResponseModel(string.Empty, message: "Server caught an error " + e.Message);
		}
	}

	public async Task<BaseResponseModel<string>> ChangePasswordAsync(ChangePasswordRequest request, string userEmail, bool IsForgot)
	{
		try
		{
			_unitOfWork.BeginTransaction();
			var users = await _unitOfWork.GetRepository<User>().FindAsync(e => e.Email.Equals(userEmail));
			if (users == null)
			{
				return BaseResponseModel<string>.NotFoundResponseModel(string.Empty, message: ErrorMessage.UserNotFound);
			}

			//If IsForgot is false, we need to check the old password
			if (!IsForgot)
			{
				if (string.IsNullOrEmpty(request.CurrentPassword))
					return BaseResponseModel<string>.BadRequestResponseModel(string.Empty, message: "Mật khẩu hiện tại không thể để trống");
				if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, users.Password))
					return BaseResponseModel<string>.BadRequestResponseModel(string.Empty, message: "Mật khẩu hiện tại không chính xác");

			}


			if (!IsValid(request.NewPassword))
				return BaseResponseModel<string>.BadRequestResponseModel(string.Empty, message: ErrorMessage.ValidatePassword);

			if (request.NewPassword != request.ConfirmNewPassword)
				
			return BaseResponseModel<string>.BadRequestResponseModel(string.Empty, message: ErrorMessage.ConfirmPasswordNotMatch);

			users.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

			await _unitOfWork.GetRepository<User>().UpdateAsync(users);
			await _unitOfWork.SaveAsync();
			_unitOfWork.CommitTransaction();

			return BaseResponseModel<string>.OkResponseModel("Success", message: "Change password successful");

		}
		catch (Exception e)
		{
			_unitOfWork.RollBack();
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<string>.InternalErrorResponseModel(string.Empty, message: "Server caught an error " + e.Message);
		}
	}

	private bool IsValid(string password)
	{
		if (password.Length < 8) return false;

		if (!Regex.IsMatch(password, @"[A-Z]")) return false;

		if (!Regex.IsMatch(password, @"[a-z]")) return false;

		if (!Regex.IsMatch(password, @"\d")) return false;

		if (!Regex.IsMatch(password, @"[@#$%^&*!_]")) return false;

		return true;
	}

	private bool IsValidPhoneNumber(string phoneNumber)
	{
		if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

		// Vietnamese phone number pattern: 
		// - Start with +84, 84, or 0
		// - Followed by 3, 5, 7, 8, or 9
		// - Then 8 more digits
		return Regex.IsMatch(phoneNumber, @"^(\+84|84|0)[3|5|7|8|9][0-9]{8}$");
	}
}
