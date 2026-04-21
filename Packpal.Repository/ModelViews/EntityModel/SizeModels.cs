using System.ComponentModel.DataAnnotations;

namespace Packpal.DAL.ModelViews.EntityModel
{
	public class UpdateSizeModel
	{
		[Required]
		public Guid Id { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "Size Description cannot exceed 100 characters")]
		public string SizeDescription { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
		public double Price { get; set; }
	}
	public class CreateSizeModel
	{
		[Required]
		[StringLength(100, ErrorMessage = "Size Description cannot exceed 100 characters")]
		public string SizeDescription { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
		public double Price { get; set; }
	}
	public class ViewSizeModel
	{
		public Guid Id { get; set; }
		public string SizeDescription { get; set; }
		public double Price { get; set; }

		//Include number of orders using this size
		public int OrderCount { get; set; }
	}

	public class SizeQuery
	{
		public string SearchTerm { get; set; } = string.Empty; // Default to empty string
		public double Price { get; set; } = 0.0; // Default to 0, meaning no price filter
		public int PageIndex { get; set; } = 1; // Default to first page
		public int PageSize { get; set; } = 5; // Default page size
	}
}
