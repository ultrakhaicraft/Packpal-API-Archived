namespace Packpal.DAL.Entity
{
    public class Rating
    {
        public Guid Id { get; set; } = Guid.NewGuid();
		public Guid RenterId { get; set; }
        public Guid StorageId { get; set; }
        public int Star { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime RatingDate { get; set; }
        //Navigation properties
        public virtual Renter? Renter { get; set; }
        public virtual Storage? Storage { get; set; }
    }
}
