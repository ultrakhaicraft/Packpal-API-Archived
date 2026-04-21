namespace Packpal.DAL.Entity
{
    public class Renter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Rating> Ratings { get; set; }
    }
}
