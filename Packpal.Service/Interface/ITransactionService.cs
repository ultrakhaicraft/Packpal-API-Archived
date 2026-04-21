using DataAccess.ResponseModel;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Packpal.BLL.Interface;

public interface ITransactionService
{
	public Task<BaseResponseModel<string>> Create(TransactionCreateModel request);
	public Task<BaseResponseModel<PagingModel<TransactionViewModel>>> GetAll(TransactionQuery query);
	public Task<BaseResponseModel<TransactionViewModel>> GetById(string id);
	public Task<BaseResponseModel<string>> ChangeStatus(string id, string status);

}
