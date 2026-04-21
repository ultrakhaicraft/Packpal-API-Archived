using Microsoft.EntityFrameworkCore;
using Packpal.DAL.Entity;

namespace Packpal.DAL.Context;

public partial class PackpalDbContext : DbContext
{
    public PackpalDbContext(DbContextOptions<PackpalDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Keeper> Keepers { get; set; }
    public DbSet<Renter> Renters { get; set; }
    public DbSet<Storage> Storages { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Size> Sizes { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<PayoutRequest> PayoutRequests { get; set; }
    public DbSet<Request> Requests { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Storage)
            .WithMany(s => s.Ratings)
            .HasForeignKey(r => r.StorageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Storage)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.StorageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>().ToTable("Transactions"); // Force plural table name

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Transactions)
            .WithOne(t => t.Order)
            .HasForeignKey(t => t.OrderId);
        /*OnModelCreatingPartial(modelBuilder);*/
    }

    /*partial void OnModelCreatingPartial(ModelBuilder modelBuilder);*/
}
