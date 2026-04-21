
using Microsoft.AspNetCore.Http;
using Packpal.DAL.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packpal.DAL.ModelViews.EntityModel;

//Authentication
public class LoginRequest
{
	[Required(ErrorMessage = "Email is required")]
	[EmailAddress(ErrorMessage = "Invalid email address format")]
	public string? Email { get; set; }
	[Required(ErrorMessage = "Password is required")]
	public string? Password { get; set; }
}

public class UserQuery
{
	public RoleEnum? Role { get; set; }
	public string? Username { get; set; }
	public int PageIndex { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public UserStatusEnum? Status { get; set; }
}

//Also use for Register
public class UserCreateRequest
{
	[Required(ErrorMessage = "Email is required")]
	[EmailAddress(ErrorMessage = "Invalid email address format")]
	public string Email { get; set; } = string.Empty;
	[Required(ErrorMessage = "Password is required")]
	public string Password { get; set; } = string.Empty;
	public string? ConfirmPassword { get; set; }
	[Required(ErrorMessage = "Username is required")]
	public string Username { get; set; } = string.Empty;
	[Required(ErrorMessage = "Phone Number is required")]
	[RegularExpression(@"^(\+84|84|0)[3|5|7|8|9][0-9]{8}$", ErrorMessage = "Invalid phone number format. Please use Vietnamese phone number format (e.g., 0901234567, +84901234567)")]
	public string PhoneNumber { get; set; } = string.Empty;

}

public class ChangePasswordRequest
{

	public string? CurrentPassword { get; set; }

	[Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
	[MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
	[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$%^&*!_])[A-Za-z\d@#$%^&*!_]{8,}$", 
		ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt (@#$%^&*!_)")]
	public required string NewPassword { get; set; }
	
	[Required(ErrorMessage = "Xác nhận mật khẩu mới là bắt buộc")]
	public required string ConfirmNewPassword { get; set; }
}

public class UserViewModel
{
	public string? Id { get; set; }
	public string? Email { get; set; }
	public string? Username { get; set; }
	public List<string>? Roles { get; set; }
	public string? ActiveRole { get; set; }
	public string? Status { get; set; }
	public string? AvatarUrl { get; set; }
}

public class UserDetailModel
{
	public string? Id { get; set; }
	public string? Email { get; set; }
	public string? Password { get; set; }
	public string? Username { get; set; }
	public string? PhoneNumber { get; set; }
	public List<string>? Roles { get; set; }
	public string? ActiveRole { get; set; }
    public string? Status { get; set; }
	public string? AvatarUrl { get; set; }
	public KeeperDetailModel? Keeper { get; set; }
	public RenterDetailModel? Renter { get; set; }
}

public class KeeperDetailModel
{
	public Guid KeeperId { get; set; }
	public string? IdentityNumber { get; set; }
	public string? Documents { get; set; }
	public string? BankAccount { get; set; }
}

public class RenterDetailModel
{
	public Guid RenterId { get; set; }
}

public class KeeperRegisterForm
{

	[Required(ErrorMessage = "Email is required")]
	[EmailAddress(ErrorMessage = "Invalid email address format")]
	public required string Email { get; set; }
	[Required(ErrorMessage = "IdentityNumber is required")]
	public required string IdentityNumber { get; set; }
    [Required(ErrorMessage = "BankAccount is required")]
    public required string BankAccount { get; set; }

	[Required(ErrorMessage = "Documents is required")]
	[DataType(DataType.Upload)]
	public required IFormFile Documents { get; set; }

}

public class UserUpdateModel
{
	[Required(ErrorMessage = "Email is required")]
	[EmailAddress(ErrorMessage = "Invalid email address format")]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "Username is required")]
	public string Username { get; set; } = string.Empty;
	[Required(ErrorMessage = "Phone Number is required")]
	public string PhoneNumber { get; set; } = string.Empty;
}

public class SwitchRoleRequest
{
	[Required(ErrorMessage = "Role is required")]
	public string Role { get; set; } = string.Empty;
}

public class UserRoleResponse
{
	public Guid UserId { get; set; }
	public string? Email { get; set; }
	public string? Username { get; set; }
	public string? PhoneNumber { get; set; }
	public List<string> Roles { get; set; } = new List<string>();
	public string? ActiveRole { get; set; }
	public string? Status { get; set; }
	public string? AvatarUrl { get; set; }
}

