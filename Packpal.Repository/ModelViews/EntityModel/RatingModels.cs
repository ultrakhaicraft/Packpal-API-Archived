using System.ComponentModel.DataAnnotations;

namespace Packpal.DAL.ModelViews.EntityModel;

public class UpdateRatingModel
{
	[Required]
	public Guid Id { get; set; }

	[Required]
	[Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
	public int Star { get; set; }

	[StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
	public string Comment { get; set; } = string.Empty;
}
public class CreateRatingModel
{
	[Required]
	public Guid RenterId { get; set; }

	[Required]
	public Guid StorageId { get; set; }

	[Required]
	[Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
	public int Star { get; set; }

	[StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
	public string Comment { get; set; } = string.Empty;
}
public class ViewRatingModel
{
	public Guid Id { get; set; }
	public Guid RenterId { get; set; }
	public Guid StorageId { get; set; }
	public int Star { get; set; }
	public string Comment { get; set; }
	public DateTime RatingDate { get; set; }

	// Additional properties from related entities
	public string RenterName { get; set; }
	public string StorageAddress { get; set; }
}

public class RatingQuery
{
	public int PageSize { get; set; } = 5;
	public int PageIndex { get; set; } = 1;
}
