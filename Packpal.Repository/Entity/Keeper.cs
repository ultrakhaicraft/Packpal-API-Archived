namespace Packpal.DAL.Entity
{
    public class Keeper
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string IdentityNumber { get; set; } = string.Empty;
        public string Documents { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public Guid UserId { get; set; }

        // Navigation property
        public virtual User? User { get; set; }
        public virtual ICollection<Storage> Storages { get; set; } = new List<Storage>();
    }
}
