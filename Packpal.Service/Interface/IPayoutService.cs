using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Http;
using Packpal.DAL.Enum;
using Packpal.DAL.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Packpal.DAL.ModelViews.PayoutInfoModel;

namespace Packpal.BLL.Interface;

public interface IPayoutService
{
	Task<BaseResponseModel<ViewPayoutInfo>> CreatePayout(CreatePayoutInfo request);
	Task<BaseResponseModel<ViewPayoutInfo>> ChangePayoutStatus(Guid payoutId, PayoutRequestStatusEnum status);
	Task<BaseResponseModel<ViewPayoutInfo>> StartProcessingPayout(Guid payoutId, Guid staffUserId);
	Task<BaseResponseModel<ViewPayoutInfo>> CompletePayout(Guid payoutId, string transactionCode, string description);
	Task<BaseResponseModel<ViewPayoutInfo>> GetAPayoutInfo(Guid payoutId);
	Task<BaseResponseModel<ViewPayoutInfo>> UploadProofToPayout(Guid payoutId, IFormFile proofImage);
	Task<BaseResponseModel<PagingModel<ViewPayoutInfo>>> GetAllPayoutRequests(int pageIndex, int pageSize, string? status);
	Task<BaseResponseModel<PagingModel<ViewPayoutInfo>>> GetPayoutRequestsByKeeper(Guid keeperId, int pageIndex, int pageSize);
	Task<BaseResponseModel<object>> CheckPayoutEligibility(Guid orderId, Guid keeperId);
	Task<BaseResponseModel<object>> GetOrderPayoutStatus(Guid orderId, Guid keeperId);
}
