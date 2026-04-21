using Packpal.DAL.Enum;

namespace Packpal.DAL.Entity
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RenterId { get; set; }
        public Guid StorageId { get; set; }

        public string Status { get; set; } = OrderStatusEnum.PENDING.ToString(); // default status is PENDING
        public double TotalAmount { get; set; }
        public string PackageDescription { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public bool IsPaid { get; set; } = false; // default is not paid
        public DateTime? StartKeepTime { get; set; }
        public int EstimatedDays { get; set; } = 1; // Planned storage duration
        public string[] OrderCertification { get; set; } = Array.Empty<string>(); // Max 2 images for package proof
        //Navigation properties
        public virtual Renter? Renter { get; set; }
        public virtual Storage? Storage { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
	}

}
