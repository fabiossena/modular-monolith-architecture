using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Cart.Domain;
using ModularMonolith.Shared.Platform.Persistence;
using CartAggregate = ModularMonolith.Modules.Cart.Domain.Cart;

namespace ModularMonolith.Modules.Cart.Infrastructure;

/// <summary>The database context of the Cart module.</summary>
public sealed class CartDbContext(
    DbContextOptions<CartDbContext> options,
    TransactionCoordinator transactionCoordinator)
    : ModuleDbContext(options, transactionCoordinator)
{
    public DbSet<CartAggregate> Carts => Set<CartAggregate>();
    public DbSet<CartLine> CartLines => Set<CartLine>();

    public override string ModuleName => "Cart";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var cart = modelBuilder.Entity<CartAggregate>();

        cart.HasKey(c => c.Id);
        cart.Property(c => c.Id).HasMaxLength(64).ValueGeneratedNever();
        cart.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(l => l.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        var line = modelBuilder.Entity<CartLine>();
        line.HasKey(l => l.Id);
        line.HasIndex(l => l.CartId);
        line.HasIndex(l => new { l.CartId, l.ProductId }).IsUnique();
    }
}
