using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.Platform.Persistence;

/// <summary>
/// Coordinates the single transaction that may span several bounded contexts.
/// <para>
/// A modular monolith keeps the atomicity of the classic monolith: the checkout
/// of the Orders module writes to the Orders, Payments, Inventory and Cart
/// tables, and either all of those writes land or none of them do. The
/// coordinator starts one transaction on the shared connection and every module
/// context enlists in it before it saves.
/// </para>
/// <para>
/// It is scoped: one request, one transaction. No business rule lives here.
/// </para>
/// </summary>
public sealed class TransactionCoordinator
{
    private readonly SqliteConnectionProvider _connectionProvider;
    private readonly List<ModuleDbContext> _enlistedContexts = new();
    private SqliteTransaction? _transaction;

    public TransactionCoordinator(SqliteConnectionProvider connectionProvider) =>
        _connectionProvider = connectionProvider;

    public bool HasActiveTransaction => _transaction is not null;

    /// <summary>Starts the transaction that every module context will enlist in.</summary>
    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already active in the current scope.");

        _transaction = (SqliteTransaction)await _connectionProvider.Connection
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Enlists a module context in the active transaction. Called by
    /// <see cref="ModuleDbContext"/> right before it saves, because EF Core
    /// would otherwise try to open a nested transaction on the shared
    /// connection, which SQLite does not support.
    /// </summary>
    public Task EnlistAsync(ModuleDbContext context, CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return Task.CompletedTask;

        foreach (var enlisted in _enlistedContexts)
        {
            if (ReferenceEquals(enlisted, context))
                return Task.CompletedTask;
        }

        context.Database.UseTransaction(_transaction);
        _enlistedContexts.Add(context);

        return Task.CompletedTask;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        try
        {
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await ReleaseAsync().ConfigureAwait(false);
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        try
        {
            await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            // The transaction was already completed (e.g. by SQLite itself).
        }
        finally
        {
            await ReleaseAsync().ConfigureAwait(false);
        }
    }

    private async Task ReleaseAsync()
    {
        var transaction = _transaction;
        _transaction = null;

        if (transaction is not null)
            await transaction.DisposeAsync().ConfigureAwait(false);

        // The contexts must forget the finished transaction, otherwise EF Core
        // keeps using it on the next save.
        foreach (var context in _enlistedContexts)
        {
            try
            {
                context.Database.UseTransaction(null);
            }
            catch (ObjectDisposedException)
            {
                // The context was already disposed by the scope.
            }
            catch (InvalidOperationException)
            {
                // The context refuses to be reset; it will not be used again.
            }
        }

        _enlistedContexts.Clear();
    }
}
