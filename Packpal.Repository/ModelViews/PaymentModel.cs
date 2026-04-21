using Packpal.DAL.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packpal.DAL.ModelViews;


// PayOS Webhook


public class PayOSWebhookRequest
{
	public string Event { get; set; }
	public long OrderCode { get; set; }
	public string Status { get; set; }
	public string Description { get; set; }
	public long Amount { get; set; }
	public string Signature { get; set; }
	public long Time { get; set; }
}


public class VerifyWebhookResponse
{
	public string Code { get; set; } = default!;
	public string Desc { get; set; } = default!;
	public object Data { get; set; } = false;
	
}



//Create Payment Link
public class PaymentRequest
{
	[Required(ErrorMessage ="Amount is required")]
	[Range(2000, int.MaxValue, ErrorMessage ="Amount must be larger than 2000 VND")]
	public required int Amount { get; set; }
	[Required(ErrorMessage = "Description is required")]
	public required string Description { get; set; }
	[Required(ErrorMessage = "Return Url is required")]
	public required string ReturnUrl { get; set; }
	[Required(ErrorMessage = "Cancel Url is required")]
	public required string CancelUrl { get; set; }

	[Required(ErrorMessage = "Order Id is required")]
	public required string OrderId { get; set; }
	public string? BuyerEmail { get; set; }
	public string? BuyerPhone { get; set; }
}







