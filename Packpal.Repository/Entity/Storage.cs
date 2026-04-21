using Packpal.DAL.Enum;
namespace Packpal.DAL.Entity
{
    public class Storage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Status { get; set; } = StorageStatusEnum.AVAILABLE.ToString(); // default status is AVAILABLE
        public string Description { get; set; } = string.Empty;
        public required string Address { get; set; }
        public Guid KeeperId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        // Navigation property
        public virtual Keeper? Keeper { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
