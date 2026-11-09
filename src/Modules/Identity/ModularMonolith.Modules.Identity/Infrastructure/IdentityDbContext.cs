using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Identity.Domain;
using ModularMonolith.Shared.Platform.Persistence;

namespace ModularMonolith.Modules.Identity.Infrastructure;

/// <summary>The database context of the Identity module.</summary>
public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options,
    TransactionCoordinator transactionCoordinator)
    : ModuleDbContext(options, transactionCoordinator)
{
    public DbSet<User> Users => Set<User>();

    public override string ModuleName => "Identity";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();

        user.HasKey(u => u.Id);
        user.Property(u => u.Id).HasMaxLength(64).ValueGeneratedNever();
        user.Property(u => u.Email).HasMaxLength(320).IsRequired();
        user.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
        user.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
        user.Property(u => u.Role).HasMaxLength(32).IsRequired();

        user.HasIndex(u => u.Email).IsUnique();
    }
}
