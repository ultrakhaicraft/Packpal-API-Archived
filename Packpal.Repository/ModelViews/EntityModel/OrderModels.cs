using Packpal.DAL.Enum;
using System.ComponentModel.DataAnnotations;
namespace Packpal.DAL.ModelViews.EntityModel;


public class OrderQuery
{
	public bool? IsPaid { get; set; } // nullable to allow filtering all orders when not specified
	public OrderStatusEnum? Status { get; set; }
	public int PageIndex { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public DateTime? MonthAndYear { get; set; }
}

public class CreateOrderModel
{
	[Required]
	public Guid RenterId { get; set; }

	[Required]
	public Guid StorageId { get; set; }

	[Required]
	public string? PackageDescription { get; set; }

	[Required]
	[Range(1, int.MaxValue, ErrorMessage = "Estimated days must be at least 1")]
	public int EstimatedDays { get; set; } = 1;

}

public class UpdateOrderModel
{
	[Required]
	public Guid Id { get; set; }
	public string? PackageDescription { get; set; }


}

// Enhanced model for PATCH operations - supports partial updates
public class PatchOrderModel
{
	[Required]
	public Guid Id { get; set; }
	
	// Optional fields for partial updates
	public string? PackageDescription { get; set; }
	
	[EnumDataType(typeof(OrderStatusEnum), ErrorMessage = "Invalid status value")]
	public string? Status { get; set; }
	
	public List<string>? OrderCertification { get; set; }
	
	[Range(1, int.MaxValue, ErrorMessage = "Estimated days must be at least 1")]
	public int? EstimatedDays { get; set; }
	
	public bool? IsPaid { get; set; }
	
	public DateTime? StartTime { get; set; }
	
	[Range(0, double.MaxValue, ErrorMessage = "Total amount must be non-negative")]
	public double? TotalAmount { get; set; }
}

public class ExtendedOrderViewModel
{
	public Guid Id { get; set; }
	public Guid RenterId { get; set; }
	public Guid StorageId { get; set; }
	[EnumDataType(typeof(OrderStatusEnum), ErrorMessage = "Invalid status value")]
	public string? Status { get; set; }
	public double TotalAmount { get; set; }
	public string PackageDescription { get; set; } = string.Empty;
	public DateTime OrderDate { get; set; }
	public bool IsPaid { get; set; }
	public DateTime? StartKeepTime { get; set; }
	public int EstimatedDays { get; set; } = 1;
	public string[] OrderCertification { get; set; } = Array.Empty<string>();

	// Related entity information
	public string RenterName { get; set; } = string.Empty;
	public string RenterEmail { get; set; } = string.Empty;
	public string RenterUsername { get; set; } = string.Empty;
	public string StorageAddress { get; set; } = string.Empty;
	public List<ViewOrderDetailModel> OrderDetails { get; set; } = new List<ViewOrderDetailModel>();
}

//View as a list
public class ViewSummaryOrderModel
{
	public Guid Id { get; set; }
	public Guid RenterId { get; set; }
	public Guid StorageId { get; set; }
	[EnumDataType(typeof(OrderStatusEnum), ErrorMessage = "Invalid status value")]
	public string? Status { get; set; }
	public double TotalAmount { get; set; }
	public string PackageDescription { get; set; } = string.Empty;
	public DateTime OrderDate { get; set; }
	public bool IsPaid { get; set; }
	
	// Navigation properties
	public RenterSummaryModel? Renter { get; set; }
}

public class RenterSummaryModel
{
	public Guid Id { get; set; }
	public string Username { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
}