using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Catalog.Domain;
using ModularMonolith.Shared.Platform.Persistence;

namespace ModularMonolith.Modules.Catalog.Infrastructure;

/// <summary>
/// The database context of the Catalog module. It only knows the tables of
/// the Catalog context — there is no reference to any other module table.
/// </summary>
public sealed class CatalogDbContext(
    DbContextOptions<CatalogDbContext> options,
    TransactionCoordinator transactionCoordinator)
    : ModuleDbContext(options, transactionCoordinator)
{
    public DbSet<Product> Products => Set<Product>();

    public override string ModuleName => "Catalog";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();

        product.HasKey(p => p.Id);
        product.Property(p => p.Sku).HasMaxLength(32).IsRequired();
        product.Property(p => p.Name).HasMaxLength(200).IsRequired();
        product.Property(p => p.Description).HasMaxLength(1000);
        product.Property(p => p.Category).HasMaxLength(100).IsRequired();
        product.Property(p => p.Emoji).HasMaxLength(16);
        product.Property(p => p.Price).HasPrecision(18, 2);

        product.HasIndex(p => p.Sku).IsUnique();
        product.HasIndex(p => p.Category);
    }
}
