using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packpal.DAL.ModelViews;

public class PayoutInfoModel
{

	//Payout Info (Manual)

	public class CreatePayoutInfo
	{
		[Required(ErrorMessage = "OrderId is required")]
		public Guid OrderId { get; set; }
		
		[Required(ErrorMessage = "KeeperId is required")]  
		public Guid KeeperId { get; set; }	// Keeper tạo request, không phải staff
		
		public string? BankAccount { get; set; }
		public string? Note { get; set; }
	}

	public class CreatePayoutFromOrderRequest
	{
		[Required(ErrorMessage = "OrderId is required")]
		public Guid OrderId { get; set; }
		
		[Required(ErrorMessage = "KeeperId is required")]
		public Guid KeeperId { get; set; }
		
		public string? BankAccount { get; set; }
		public string? Note { get; set; }
	}

	public class ViewPayoutInfo
	{
		public Guid Id { get; set; } 

		public Guid OrderId { get; set; }
		public Guid? UserId { get; set; }
		public Guid? KeeperId { get; set; }
		public Guid? TransactionId { get; set; }

		public double Amount { get; set; }
		public DateTime CreatedAt { get; set; }
		public string? Status { get; set; }

		public string? ImageUrl { get; set; }

		// Add keeper information
		public KeeperInfo? Keeper { get; set; }
	}

	public class KeeperInfo
	{
		public string? Username { get; set; }
		public string? Email { get; set; }
		public string? BankAccount { get; set; }
		public string? FullName { get; set; }
	}




	// Payout for the PayOS (Upcoming feature)
	/*
	public class PartialPayoutRequest
	{

		[Required(ErrorMessage = "Amount is required")]
		[Range(1000, int.MaxValue, ErrorMessage = "Amount must be larger than 1000 VND")]
		public int Amount { get; set; }
		[Required(ErrorMessage = "Account Number Received is required")]
		public string AccountNumber { get; set; } = default!;
		[Required(ErrorMessage = "Bank Code Destination (ToBin) is required")]
		public string BankCode { get; set; } = default!;


	}
	public class PayOutRequest
	{
		[Required(ErrorMessage = "References ID is required")]
		public string ReferenceId { get; set; } = default!;
		[Required(ErrorMessage = "Amount is required")]
		[Range(1000, int.MaxValue, ErrorMessage = "Amount must be larger than 1000 VND")]
		public int Amount { get; set; }
		[Required(ErrorMessage = "Description is required")]
		public string Description { get; set; } = default!;
		[Required(ErrorMessage = "Account Number Received is required")]
		public string ToAccountNumber { get; set; } = default!;
		[Required(ErrorMessage = "Bank Code Destination (ToBin) is required")]
		public string ToBin { get; set; } = default!;
		[Required(ErrorMessage = "Array of Category is required")]
		public string[] Category { get; set; } = default!;
	}

	public class PayOutResponse
	{
		public string Code { get; set; } = default!;
		public string Desc { get; set; } = default!;
		public object? Data { get; set; } = default!;
	}

	*/
}
