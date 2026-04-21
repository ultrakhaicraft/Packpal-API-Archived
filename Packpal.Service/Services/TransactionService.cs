using DataAccess.ResponseModel;
using Packpal.BLL.Interface;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.ModelViews;
using Packpal.DAL.ModelViews.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packpal.BLL.Services;

public class TransactionService : ITransactionService
{
	private readonly IUnitOfWork _unitOfWork;

	public TransactionService(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	
	//Check if the order id request has transaction that has completed status
	public async Task<BaseResponseModel<string>> Create(TransactionCreateModel request)
	{
		try
		{
			var newTransaction = new Transaction
			{
				Amount = request.Amount,
				Description = request.Description,
				OrderId = new Guid(request.OrderId),
				TransactionCode = request.TransactionCode,

			};
			await _unitOfWork.GetRepository<Transaction>().InsertAsync(newTransaction);
			await _unitOfWork.SaveAsync();
			return BaseResponseModel<string>.OkResponseModel(data: newTransaction.Id.ToString(), message: "Transaction created successfully");
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<string>.InternalErrorResponseModel(null, message: "Server caught an error " + e.Message);
		}
	}

	public async Task<BaseResponseModel<PagingModel<TransactionViewModel>>> GetAll(TransactionQuery query)
	{
		try
		{
			var transactions = await _unitOfWork.GetRepository<Transaction>().GetAllAsync();
			if (transactions == null || !transactions.Any())
			{
				return BaseResponseModel<PagingModel<TransactionViewModel>>.NotFoundResponseModel(null, message: "No Transactions found");
			}
			var transactionViewModels = transactions.Select(t => new TransactionViewModel
			{
				Id = t.Id.ToString(),
				Amount = t.Amount,
				Description = t.Description,
				OrderId = t.OrderId.ToString(),
				TransactionCode = t.TransactionCode,
				Status = t.Status
			});

			var pagedData = PagingExtension.ToPagingModel(transactionViewModels, query.PageIndex, query.PageSize);


			return BaseResponseModel<PagingModel<TransactionViewModel>>.OkResponseModel(data: pagedData, message: "Transactions retrieved successfully");

		}
		catch (Exception e)
		{

			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<PagingModel<TransactionViewModel>>.InternalErrorResponseModel(null, message: "Server caught an error " + e.Message); ;
		}
	}

	public async Task<BaseResponseModel<TransactionViewModel>> GetById(string id)
	{
		try
		{
			var transaction = await _unitOfWork.GetRepository<Transaction>().GetByIdAsync(new Guid(id));
			if (transaction == null)
			{
				return BaseResponseModel<TransactionViewModel>.NotFoundResponseModel(null, message: "This Transaction not found with the ID");
			}
			var transactionViewModel = new TransactionViewModel
			{
				Id = transaction.Id.ToString(),
				Amount = transaction.Amount,
				Description = transaction.Description,
				OrderId = transaction.OrderId.ToString(),
				TransactionCode = transaction.TransactionCode,
				Status = transaction.Status
			};

			return BaseResponseModel<TransactionViewModel>.OkResponseModel(data: transactionViewModel, message: "Transaction retrieved successfully");
		}
		catch (Exception e)
		{

			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<TransactionViewModel>.InternalErrorResponseModel(null, message: "Server caught an error " + e.Message); ;
		}
	}

	public async Task<BaseResponseModel<string>> ChangeStatus(string id, string status)
	{
		try
		{
			var transaction = await _unitOfWork.GetRepository<Transaction>().GetByIdAsync(new Guid(id));
			if (transaction == null)
			{
				return BaseResponseModel<string>.NotFoundResponseModel(null, message: "This Transaction not found with the ID");
			}
			transaction.Status = status;
			await _unitOfWork.GetRepository<Transaction>().UpdateAsync(transaction);
			await _unitOfWork.SaveAsync();
			return BaseResponseModel<string>.OkResponseModel(data: transaction.Id.ToString(), message: "Transaction status updated successfully");
		}
		catch (Exception e)
		{

			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return BaseResponseModel<string>.InternalErrorResponseModel(null, message: "Server caught an error " + e.Message); 
		}

		
	}
}
