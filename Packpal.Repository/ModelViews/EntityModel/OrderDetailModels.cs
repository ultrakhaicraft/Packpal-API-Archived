using System.ComponentModel.DataAnnotations;

namespace Packpal.DAL.ModelViews.EntityModel
{
	public class ViewOrderDetailModel
	{
		public Guid Id { get; set; }
		public Guid SizeId { get; set; }
		public Guid OrderId { get; set; }
		//additional properties from related entities
		public string SizeDescription { get; set; } // take from size
		public double Price { get; set; } // take from size
		public string OrderStatus { get; set; } // take from order
	}

	//Don't need to input OrderID
	public class CreateOrderDetailModel
	{
		[Required]
		public Guid SizeId { get; set; }

	}

	public class UpdateOrderDetailModel
	{
		[Required]
		public Guid Id { get; set; }

		[Required]
		public Guid SizeId { get; set; }
	}
}
