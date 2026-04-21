using Packpal.DAL.Enum;

namespace Packpal.DAL.Entity
{
    public class Request
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; } = RequestStatusEnum.PENDING.ToString();
        public string? Data { get; set; } // JSON string to store request-specific data
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedBy { get; set; }

        public virtual User? User { get; set; }
    }
}
