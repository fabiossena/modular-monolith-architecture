using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Inventory.Domain;
using ModularMonolith.Shared.Platform.Persistence;

namespace ModularMonolith.Modules.Inventory.Infrastructure;

/// <summary>
/// The database context of the Inventory module. The product id is stored as a
/// plain value: there is no foreign key towards the Catalog tables, because
/// the Catalog owns its data.
/// </summary>
public sealed class InventoryDbContext(
    DbContextOptions<InventoryDbContext> options,
    TransactionCoordinator transactionCoordinator)
    : ModuleDbContext(options, transactionCoordinator)
{
    public DbSet<StockItem> StockItems => Set<StockItem>();

    public override string ModuleName => "Inventory";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var stockItem = modelBuilder.Entity<StockItem>();

        stockItem.HasKey(i => i.Id);
        stockItem.Property(i => i.ProductId).IsRequired();
        stockItem.HasIndex(i => i.ProductId).IsUnique();
    }
}
