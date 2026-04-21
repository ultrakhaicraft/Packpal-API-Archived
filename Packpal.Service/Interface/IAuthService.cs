using DataAccess.ResponseModel;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packpal.BLL.Interface;

public interface IAuthService
{
	Task<BaseResponseModel<JWTToken>> LoginAsync(LoginRequest model);
	Task<BaseResponseModel<string>> RegisterAsync(UserCreateRequest model);
	Task<BaseResponseModel<string>> ChangePasswordAsync(ChangePasswordRequest request, string userEmail, bool IsForgot);
}
