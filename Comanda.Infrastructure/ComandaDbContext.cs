using Comanda.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comanda.Infrastructure;

public class ComandaDbContext : DbContext
{
    public ComandaDbContext(DbContextOptions<ComandaDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("comanda");

        modelBuilder.Entity<Order>()
            .HasOne(o => o.CreatedByUser)
            .WithMany()
            .HasForeignKey(o => o.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne<Order>()
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
