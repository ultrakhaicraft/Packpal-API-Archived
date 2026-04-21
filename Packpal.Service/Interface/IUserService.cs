using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Http;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;

namespace Packpal.BLL.Interface;

public interface IUserService
{

	Task<BaseResponseModel<PagingModel<UserViewModel>>> GetAllAccountsAsync(UserQuery query);
	Task<BaseResponseModel<UserDetailModel>> GetAccountDetailAsync(string userId);
	Task<BaseResponseModel<string>> CreateAsync(UserCreateRequest request);
	Task<BaseResponseModel<string>> UpdateAsync(UserUpdateModel model, string userId);
	Task<BaseResponseModel<string>> DeleteAsync(string accountId);
	Task<BaseResponseModel<string>> RegisterKeeper(KeeperRegisterForm request, IFormFile file);
	
	/// <summary>
	/// Register keeper from request data (file already uploaded)
	/// </summary>
	/// <param name="userId">User ID to upgrade to keeper</param>
	/// <param name="keeperData">Keeper registration data from request</param>
	/// <returns></returns>
	Task<BaseResponseModel<string>> RegisterKeeperFromRequestAsync(Guid userId, KeeperRegistrationData keeperData);
	
    Task<BaseResponseModel<string>> BanAsync(string accountId);
	Task<BaseResponseModel<UserViewModel>> UpdateAvatarUrl(Guid userId, IFormFile avatarImage);
	Task<BaseResponseModel<UserRoleResponse>> SwitchRole(string userEmail, SwitchRoleRequest request);

}
