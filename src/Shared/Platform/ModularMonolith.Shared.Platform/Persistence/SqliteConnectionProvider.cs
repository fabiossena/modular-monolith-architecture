using Microsoft.Data.Sqlite;

namespace ModularMonolith.Shared.Platform.Persistence;

/// <summary>
/// Owns the single SQLite connection that every module context of the current
/// request shares.
/// <para>
/// Sharing one physical connection is what makes a cross-module transaction
/// possible in a modular monolith: a <see cref="Microsoft.Data.Sqlite.SqliteTransaction"/>
/// is bound to the connection that created it, so all module contexts must
/// enlist in the very same connection to take part in it.
/// </para>
/// <para>
/// The connection is opened eagerly and never closed by EF Core (the contexts
/// are configured with <c>contextOwnsConnection: false</c>), so the lifetime is
/// fully controlled here.
/// </para>
/// </summary>
public sealed class SqliteConnectionProvider : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteConnectionProvider(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A SQLite connection string is required.", nameof(connectionString));

        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        ApplyPragmas();
    }

    public SqliteConnection Connection => _connection;

    private void ApplyPragmas()
    {
        using var command = _connection.CreateCommand();
        // WAL allows readers while a writer holds the lock; the busy timeout
        // makes concurrent writers wait instead of failing immediately.
        command.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000; PRAGMA foreign_keys=ON;";
        command.ExecuteNonQuery();
    }

    public void Dispose() => _connection.Dispose();
}
