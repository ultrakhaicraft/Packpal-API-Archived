namespace Packpal.DAL.Entity
{
    public class OrderDetail
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SizeId { get; set; }
        public Guid OrderId { get; set; }
        //Navigation properties
        public virtual Size? Size { get; set; }
        public virtual Order? Order { get; set; }
    }
}
