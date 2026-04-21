using Packpal.BLL.Interface;
using Packpal.DAL.ModelViews;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using System.Text.RegularExpressions;
using Packpal.DAL.Entity;
using Packpal.DAL.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Packpal.DAL.ModelViews.EntityModel;
using DataAccess.ResponseModel;




namespace Packpal.BLL.Services;

public class UserService : IUserService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IFirebaseStorageService _fileUploadService;
	

	public UserService(IUnitOfWork unitOfWork, IFirebaseStorageService fileUploadService )
	{
		_unitOfWork = unitOfWork;
		_fileUploadService = fileUploadService;
	}

	
	
	
	public async Task<BaseResponseModel<PagingModel<UserViewModel>>> GetAllAccountsAsync(UserQuery query)
	{
		try
		{
			var users = await _unitOfWork.GetRepository<User>().GetAllAsync();

			if (users == null || !users.Any())
			{
				return BaseResponseModel<PagingModel<UserViewModel>>.NotFoundResponseModel(null,message: "List of user data is empty");
			}

			//Filter by role - Updated for multi-role support
			if (query.Role.HasValue)
			{
				var roleString = query.Role.Value.ToString();
				var statusString = query.Status.HasValue ? query.Status.Value.ToString() : null;
                
                // Filter by active role or if user has the role in their roles list
                users = users.Where(user => 
                    (user.ActiveRole == roleString || (user.Roles != null && user.Roles.Contains(roleString)))
                    && (statusString == null || user.Status == statusString));
			}


			//Search by username
			if (!string.IsNullOrEmpty(query.Username))
			{
				users = users.Where(user => EF.Functions.Like(user.Username, $"%{query.Username}%"));
			}


			var userViewModels = users.Select(user => new UserViewModel
			{
				Id = user.Id.ToString(),
				Email = user.Email,
				Username = user.Username,
				Roles = user.Roles ?? new List<string> { RoleEnum.RENTER.ToString() },
				ActiveRole = user.ActiveRole ?? RoleEnum.RENTER.ToString(),
				Status = user.Status,
				AvatarUrl = user.AvatarUrl
			});

			var pagedData = PagingExtension.ToPagingModel(userViewModels, query.PageIndex, query.PageSize);

			
			return BaseResponseModel<PagingModel<UserViewModel>>.OkResponseModel(pagedData, message: "Get all user data completed");
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<PagingModel<UserViewModel>>.InternalErrorResponseModel(null, message: "Server caught an error "+e.Message);
		}

	}

	public async Task<BaseResponseModel<UserViewModel>> UpdateAvatarUrl(Guid userId, IFormFile avatarImage)
	{
		try
		{
			var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(userId);
			if (user == null)
			{
				return BaseResponseModel<UserViewModel>.NotFoundResponseModel(null, message: "This User not found given this ID");
			}

			//Need a way to delete the old avatar in Firebase and use the new one, basically replacing
			UploadResponse result = await _fileUploadService.UploadFileAsync(avatarImage);
			string avatarUrl = result.FileUrl;
			user.AvatarUrl = avatarUrl;
			await _unitOfWork.GetRepository<User>().UpdateAsync(user);
			await _unitOfWork.SaveAsync();

			var userViewModels = new UserViewModel
			{
				Id = user.Id.ToString(),
				Email = user.Email,
				Username = user.Username,
				Roles = user.Roles ?? new List<string> { RoleEnum.RENTER.ToString() },
				ActiveRole = user.ActiveRole ?? RoleEnum.RENTER.ToString(),
				Status = user.Status,
				AvatarUrl =user.AvatarUrl
			};

			return BaseResponseModel<UserViewModel>.OkResponseModel(userViewModels, message: "Update image avatar success");

		}
		catch (Exception e)
		{

			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<UserViewModel>.InternalErrorResponseModel(null, message: "Server caught an error " + e.Message);
		}
	}

	public async Task<BaseResponseModel<UserDetailModel>> GetAccountDetailAsync(string userId)
	{
		try
		{
			var users = await _unitOfWork.GetRepository<User>().GetByIdAsync(new Guid(userId));

			if (users == null)
			{				
				return BaseResponseModel<UserDetailModel>.NotFoundResponseModel(null, message: "This User not found given this ID");
			}

			var userDetailModel = new UserDetailModel
			{

				Id = users.Id.ToString(),
				Email = users.Email,
				Username = users.Username,
				PhoneNumber = users.PhoneNumber,
				Roles = users.Roles ?? new List<string> { RoleEnum.RENTER.ToString() },
				ActiveRole = users.ActiveRole ?? RoleEnum.RENTER.ToString(),
				Status = users.Status,
				AvatarUrl = users.AvatarUrl
			};

			//Load identity data based on available roles (multi-role support)
			foreach (var role in userDetailModel.Roles ?? new List<string>())
			{
				switch (role)
				{
					case "KEEPER":
						userDetailModel = await AddKeeperDetail(users.Id, userDetailModel);
						break;
					case "RENTER":
						userDetailModel = await AddRenterDetail(users.Id, userDetailModel);
						break;
					default:
						break;
				}
			}

			return BaseResponseModel<UserDetailModel>.OkResponseModel(userDetailModel, message: "Getting Account Detail done"); ;
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<UserDetailModel>.InternalErrorResponseModel(null, message: "Server caught an error " + e.Message);
		}
	}

	public async Task<BaseResponseModel<string>> CreateAsync(UserCreateRequest request)
	{
		try
		{
			_unitOfWork.BeginTransaction();
			//Validation Area
			var existingUser = _unitOfWork.GetRepository<User>().Find(x => x.Email == request.Email);

			if (existingUser != null)
				
				return BaseResponseModel<string>.BadRequestResponseModel(null, message: ErrorMessage.AccountExist);

			if (!IsValid(request.Password))
				
				return BaseResponseModel<string>.BadRequestResponseModel(null, message: ErrorMessage.ValidatePassword);

			if (request.Password != request.ConfirmPassword)
				
				return BaseResponseModel<string>.BadRequestResponseModel(null, message: ErrorMessage.ConfirmPasswordNotMatch);


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

			
			return BaseResponseModel<string>.OkResponseModel(user.Id.ToString(), message: "Create User Success, returning ID"); ;
		}
		catch (Exception e)
		{
			_unitOfWork.RollBack();
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<string>.InternalErrorResponseModel(null, message: "Server caught an error " + e.Message);
		}

	}

	public async Task<BaseResponseModel<string>> UpdateAsync(UserUpdateModel model, string userId)
	{
		try
		{
			

			if (string.IsNullOrEmpty(userId))
			{
				
				return BaseResponseModel<string>.BadRequestResponseModel(null, message: "This User ID cannot be null or empty");
			}

			var account = await _unitOfWork.GetRepository<User>().GetByIdAsync(new Guid(userId));
			if (account == null)
			{
				
				return BaseResponseModel<string>.BadRequestResponseModel(null, message: "Account not found with this ID");
			}

			//Validation Area
			var existingUser = _unitOfWork.GetRepository<User>().Find(x => x.Email == model.Email && x.Id != account.Id);
			if (existingUser != null)
			{
				return BaseResponseModel<string>.BadRequestResponseModel(null, message: ErrorMessage.AccountExist);
			}
				
			//Update User
			account.Email = model.Email;
			account.Username = model.Username;
			account.PhoneNumber = model.PhoneNumber;

			await _unitOfWork.GetRepository<User>().UpdateAsync(account);

			await _unitOfWork.SaveAsync();

			
			return BaseResponseModel<string>.OkResponseModel(userId, message: "Update User Success, returning ID"); 
		}
		catch (Exception e)
		{
			
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<string>.InternalErrorResponseModel(null, message: "Server caught an error " + e.Message);

		}
	}

	public async Task<BaseResponseModel<string>> DeleteAsync(string accountId)
	{
		try
		{
			var account = _unitOfWork.GetRepository<User>().Find(x => x.Id == new Guid(accountId));
			if (account == null)
			{
				
				return BaseResponseModel<string>.NotFoundResponseModel("false", message: "User not found");

			}
			account.Status = UserStatusEnum.INACTIVE.ToString();
            _unitOfWork.GetRepository<User>().Update(account);
			await _unitOfWork.SaveAsync();

			return BaseResponseModel<string>.OkResponseModel("true", message: "Delete user with ID " + accountId + " success");
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<string>.InternalErrorResponseModel("false", message: "Server caught an error " + e.Message);

		}


	}

	public async Task<BaseResponseModel<string>> RegisterKeeper(KeeperRegisterForm request, IFormFile file)
	{
		try
		{
			_unitOfWork.BeginTransaction();
			//Check if duplicate IdentityNumber
			var existingKeeper = _unitOfWork.GetRepository<Keeper>().Find(x => x.IdentityNumber == request.IdentityNumber);
			if (existingKeeper != null)
			{
				
				return BaseResponseModel<string>.BadRequestResponseModel("false", message: ErrorMessage.KeeperExist);

			}

			//Check if user exist or not and check if they are renter or not
			var currentUser = _unitOfWork.GetRepository<User>().Find(x => x.Email == request.Email);
			if (currentUser == null)
			{
				return BaseResponseModel<string>.NotFoundResponseModel("false", message: "User not found");
			}

			// Check if user is already a KEEPER
			if (currentUser.Roles != null && currentUser.Roles.Contains(RoleEnum.KEEPER.ToString()))
			{
				return BaseResponseModel<string>.BadRequestResponseModel("false", message: "This user is already registered as a KEEPER");
			}

			// Add KEEPER role to existing roles
			var roles = currentUser.Roles ?? new List<string>();
			if (!roles.Contains(RoleEnum.KEEPER.ToString()))
			{
				roles.Add(RoleEnum.KEEPER.ToString());
				currentUser.Roles = roles;
			}

			// Set active role to KEEPER
			currentUser.ActiveRole = RoleEnum.KEEPER.ToString();

			await _unitOfWork.GetRepository<User>().UpdateAsync(currentUser);

			UploadResponse uploadResult = await _fileUploadService.UploadFileAsync(file);
			string documentUrl=uploadResult.FileUrl;

			//Create new Keeper Identity
			var keeper = new Keeper
			{
				UserId = currentUser.Id,
				IdentityNumber = request.IdentityNumber,
				BankAccount = request.BankAccount,
                Documents = documentUrl
			};
			await _unitOfWork.GetRepository<Keeper>().InsertAsync(keeper);

			// Keep existing Renter Identity - don't remove for multi-role support
			// This allows user to switch between RENTER and KEEPER roles

			

			await _unitOfWork.SaveAsync();

			_unitOfWork.CommitTransaction();

			return BaseResponseModel<string>.OkResponseModel("true", message: "Register new Keeper with user " + currentUser.Username + " success");

		}
		catch (Exception e)
		{
			_unitOfWork.RollBack();
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<string>.InternalErrorResponseModel("false", message: "Server caught an error " + e.Message);
		}
	}

	/// <summary>
	/// Register keeper from request data (file already uploaded)
	/// </summary>
	public async Task<BaseResponseModel<string>> RegisterKeeperFromRequestAsync(Guid userId, KeeperRegistrationData keeperData)
	{
		try
		{
			_unitOfWork.BeginTransaction();
			
			// Check if duplicate IdentityNumber
			var existingKeeper = _unitOfWork.GetRepository<Keeper>().Find(x => x.IdentityNumber == keeperData.IdentityNumber);
			if (existingKeeper != null)
			{
				return BaseResponseModel<string>.BadRequestResponseModel("false", message: ErrorMessage.KeeperExist);
			}

			// Get user by ID
			var currentUser = await _unitOfWork.GetRepository<User>().GetByIdAsync(userId);
			if (currentUser == null)
			{
				return BaseResponseModel<string>.NotFoundResponseModel("false", message: "User not found");
			}

			// Check if user is already a KEEPER
			if (currentUser.Roles != null && currentUser.Roles.Contains(RoleEnum.KEEPER.ToString()))
			{
				return BaseResponseModel<string>.BadRequestResponseModel("false", message: "This user is already registered as a KEEPER");
			}

			// Add KEEPER role to existing roles
			var roles = currentUser.Roles ?? new List<string>();
			if (!roles.Contains(RoleEnum.KEEPER.ToString()))
			{
				roles.Add(RoleEnum.KEEPER.ToString());
				currentUser.Roles = roles;
			}

			// Set active role to KEEPER
			currentUser.ActiveRole = RoleEnum.KEEPER.ToString();

			await _unitOfWork.GetRepository<User>().UpdateAsync(currentUser);

			// Create new Keeper Identity with data from request
			var keeper = new Keeper
			{
				UserId = currentUser.Id,
				IdentityNumber = keeperData.IdentityNumber,
				BankAccount = keeperData.BankAccount,
				Documents = keeperData.DocumentsUrl // File already uploaded
			};
			await _unitOfWork.GetRepository<Keeper>().InsertAsync(keeper);

			await _unitOfWork.SaveAsync();
			_unitOfWork.CommitTransaction();

			return BaseResponseModel<string>.OkResponseModel("true", message: "Register new Keeper with user " + currentUser.Username + " success");
		}
		catch (Exception e)
		{
			_unitOfWork.RollBack();
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<string>.InternalErrorResponseModel("false", message: "Server caught an error " + e.Message);
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

	private async Task<UserDetailModel> AddKeeperDetail(Guid userId, UserDetailModel userDetailModel)
	{
		var keeper = await _unitOfWork.GetRepository<Keeper>().FindAsync(k => k.UserId == userId);
		if (keeper != null)
		{
			var keeperDetail = new KeeperDetailModel
			{
				KeeperId = keeper.Id,
				IdentityNumber = keeper.IdentityNumber,
				Documents = keeper.Documents,
				BankAccount = keeper.BankAccount
            };

			userDetailModel.Keeper = keeperDetail;
		}

		return userDetailModel;
	}

	private async Task<UserDetailModel> AddRenterDetail(Guid userId, UserDetailModel userDetailModel)
	{
		var renter = await _unitOfWork.GetRepository<Renter>().FindAsync(r => r.UserId == userId);
		if (renter != null)
		{
			var renterDetail = new RenterDetailModel
			{
				RenterId = renter.Id,
			};
			userDetailModel.Renter = renterDetail;
		}
		return userDetailModel;
	}

    public async Task<BaseResponseModel<string>> BanAsync(string accountId)
    {
        try
        {
            var account = _unitOfWork.GetRepository<User>().Find(x => x.Id == new Guid(accountId));
            if (account == null)
            {

                return BaseResponseModel<string>.NotFoundResponseModel("false", message: "User not found");

            }
			account.Status = UserStatusEnum.BANNED.ToString();
            _unitOfWork.GetRepository<User>().Update(account);
            await _unitOfWork.SaveAsync();

            return BaseResponseModel<string>.OkResponseModel("true", message: "Delete user with ID " + accountId + " success");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            return BaseResponseModel<string>.InternalErrorResponseModel("false", message: "Server caught an error " + e.Message);

        }
    }

	public async Task<BaseResponseModel<UserRoleResponse>> SwitchRole(string userEmail, SwitchRoleRequest request)
	{
		try
		{
			var user = _unitOfWork.GetRepository<User>().Find(x => x.Email == userEmail);
			if (user == null)
			{
				return BaseResponseModel<UserRoleResponse>.NotFoundResponseModel(null, message: "User not found");
			}

			// Check if user has the requested role
			if (user.Roles == null || !user.Roles.Contains(request.Role))
			{
				return BaseResponseModel<UserRoleResponse>.BadRequestResponseModel(null, 
					message: $"User does not have {request.Role} role. Available roles: {string.Join(", ", user.Roles ?? new List<string>())}");
			}

			// Switch active role
			user.ActiveRole = request.Role;

			await _unitOfWork.GetRepository<User>().UpdateAsync(user);
			await _unitOfWork.SaveAsync();

			var response = new UserRoleResponse
			{
				UserId = user.Id,
				Email = user.Email,
				Username = user.Username,
				PhoneNumber = user.PhoneNumber,
				Roles = user.Roles ?? new List<string>(),
				ActiveRole = user.ActiveRole,
				Status = user.Status,
				AvatarUrl = user.AvatarUrl
			};

			return BaseResponseModel<UserRoleResponse>.OkResponseModel(response, 
				message: $"Successfully switched to {request.Role} role");
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<UserRoleResponse>.InternalErrorResponseModel(null, 
				message: "Server caught an error " + e.Message);
		}
	}
}
