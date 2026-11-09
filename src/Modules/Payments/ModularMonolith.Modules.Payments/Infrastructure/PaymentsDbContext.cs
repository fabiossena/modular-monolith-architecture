using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Payments.Domain;
using ModularMonolith.Shared.Platform.Persistence;

namespace ModularMonolith.Modules.Payments.Infrastructure;

/// <summary>
/// The database context of the Payments module. The order id is a plain
/// value: the Payments module never joins the Orders tables.
/// </summary>
public sealed class PaymentsDbContext(
    DbContextOptions<PaymentsDbContext> options,
    TransactionCoordinator transactionCoordinator)
    : ModuleDbContext(options, transactionCoordinator)
{
    public DbSet<Payment> Payments => Set<Payment>();

    public override string ModuleName => "Payments";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var payment = modelBuilder.Entity<Payment>();

        payment.HasKey(p => p.Id);
        payment.Property(p => p.Amount).HasPrecision(18, 2);
        payment.Property(p => p.Method).HasMaxLength(32).IsRequired();
        payment.Property(p => p.TransactionId).HasMaxLength(64);
        payment.HasIndex(p => p.OrderId);
    }
}
