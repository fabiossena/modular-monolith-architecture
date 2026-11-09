using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Orders.Domain;
using ModularMonolith.Shared.Platform.Persistence;

namespace ModularMonolith.Modules.Orders.Infrastructure;

/// <summary>
/// The database context of the Orders module. It holds the orders and their
/// lines; the payment of an order lives in the Payments module.
/// </summary>
public sealed class OrdersDbContext(
    DbContextOptions<OrdersDbContext> options,
    TransactionCoordinator transactionCoordinator)
    : ModuleDbContext(options, transactionCoordinator)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public override string ModuleName => "Orders";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<Order>();

        order.HasKey(o => o.Id);
        order.Property(o => o.Code).HasMaxLength(32);
        order.Property(o => o.CustomerName).HasMaxLength(200).IsRequired();
        order.Property(o => o.Email).HasMaxLength(320).IsRequired();
        order.Property(o => o.Address).HasMaxLength(200);
        order.Property(o => o.City).HasMaxLength(120);
        order.Property(o => o.Zip).HasMaxLength(32);
        order.Property(o => o.Total).HasPrecision(18, 2);
        order.HasIndex(o => o.Email);
        order.HasIndex(o => o.CreatedAt);

        order.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        var item = modelBuilder.Entity<OrderItem>();
        item.HasKey(i => i.Id);
        item.Property(i => i.Name).HasMaxLength(200);
        item.Property(i => i.Sku).HasMaxLength(32);
        item.Property(i => i.Emoji).HasMaxLength(16);
        item.Property(i => i.UnitPrice).HasPrecision(18, 2);
    }
}
