namespace Packpal.DAL.Entity
{
    public class Size
    {
        public Guid Id { get; set; } = Guid.NewGuid();
		public string SizeDescription { get; set; } = string.Empty; 
        public double Price { get; set; }
        //Navigation properties
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

}
