using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.Platform.Persistence;

/// <summary>
/// Base class of every module database context.
/// <para>
/// A module context only ever touches the tables of its own bounded context:
/// there is no shared schema and no foreign key towards another module. The
/// only thing it shares with the other modules is the physical connection, and
/// the <see cref="TransactionCoordinator"/> that turns several contexts into
/// one unit of work.
/// </para>
/// </summary>
public abstract class ModuleDbContext : DbContext
{
    private readonly TransactionCoordinator _transactionCoordinator;

    protected ModuleDbContext(DbContextOptions options, TransactionCoordinator transactionCoordinator)
        : base(options)
    {
        _transactionCoordinator = transactionCoordinator;
    }

    /// <summary>The name of the module that owns this context. Used for logging.</summary>
    public abstract string ModuleName { get; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _transactionCoordinator.EnlistAsync(this, cancellationToken).ConfigureAwait(false);
        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        await _transactionCoordinator.EnlistAsync(this, cancellationToken).ConfigureAwait(false);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
    }
}
