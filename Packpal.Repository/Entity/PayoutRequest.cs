using Packpal.DAL.Enum;

namespace Packpal.DAL.Entity
{
    public class PayoutRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        // Foreign key to Order
        public Guid OrderId { get; set; }
        public Guid? UserId { get; set; }    // Staff who transfers the payout
        public Guid? TransactionId { get; set; }    // OUT transaction for payout
        public string? ImageURL { get; set; }
        public double Amount { get; set; } = 0.0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = PayoutRequestStatusEnum.NOTPAID.ToString();

        public virtual Order? Order { get; set; }
        public virtual User? User { get; set; }
        public virtual Transaction? Transaction { get; set; }
    }
}
