using Packpal.DAL.Enum;
using System.ComponentModel.DataAnnotations;

namespace Packpal.DAL.ModelViews.EntityModel;

public class ViewStorageModel
{
    public Guid Id { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public Guid KeeperId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? KeeperName { get; set; }
    public string? KeeperPhoneNumber { get; set; }
    public double AverageRating { get; set; } = 0.0;// Calculated from Ratings
    public int RatingCount { get; set; } = 0;
    public int PendingOrdersCount { get; set; } = 0; // Count of orders with PENDING status
}
public class CreateStorageModel
{
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Keeper ID is required")]
    public Guid KeeperId { get; set; }
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
}
public class UpdateStorageModel
{
    [Required(ErrorMessage = "Storage ID is required")]
    public Guid Id { get; set; }

    [EnumDataType(typeof(StorageStatusEnum), ErrorMessage = "Invalid status value")]
    public string Status { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class UpdateStorageRequestModel
{
    [EnumDataType(typeof(StorageStatusEnum), ErrorMessage = "Invalid status value")]
    public string Status { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class StorageQuery
{
    [EnumDataType(typeof(StorageStatusEnum), ErrorMessage = "Invalid status value")]
    public StorageStatusEnum? Status { get; set; }
    public string? Address { get; set; }
    public int PageSize { get; set; } = 10;
    public int PageIndex { get; set; } = 1;
}


