using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packpal.DAL.ModelViews.EntityModel;



	//Transaction
	public class TransactionQuery
	{
		public int PageIndex { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}

	public class TransactionCreateModel
	{
		[Required(ErrorMessage = "Amount is required")]
		[Range(1000, double.MaxValue, ErrorMessage = "Amount must be larger than 1000 VND")]

		public required double Amount { get; set; }
		[Required(ErrorMessage = "Description is required")]
		public required string Description { get; set; }
		[Required(ErrorMessage = "Order Id is required")]
		public required string OrderId { get; set; }
		[Required(ErrorMessage = "Transaction Code is required")]
		public required string TransactionCode { get; set; } // This is the payment/order code that is used to create the payment link
	

	}

	public class TransactionUpdateModel
	{
		[Required(ErrorMessage = "Amount is required")]
		[Range(1000, int.MaxValue, ErrorMessage = "Amount must be larger than 1000 VND")]

		public required int Amount { get; set; }
		[Required(ErrorMessage = "Description is required")]
		public required string Description { get; set; }
		[Required(ErrorMessage = "Order Id is required")]
		public required string OrderId { get; set; }
		[Required(ErrorMessage = "Transaction Code is required")]
		public required string TransactionCode { get; set; } // This is the payment/order code that is used to create the payment link
		[Required(ErrorMessage = "SenderBankAccount is required")]
		public string SenderBankAccount { get; set; } = string.Empty; // Bank Account of the sender
		[Required(ErrorMessage = "ReceiverBankAccount is required")]
		public string ReceiverBankAccount { get; set; } = string.Empty; //Bank Account of the receiver

	}



	public class TransactionViewModel
	{
		public string? Id { get; set; }
		public string? OrderId { get; set; }
		public string? TransactionCode { get; set; }
		public double Amount { get; set; }
		public string? Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public string? Status { get; set; }

	}

