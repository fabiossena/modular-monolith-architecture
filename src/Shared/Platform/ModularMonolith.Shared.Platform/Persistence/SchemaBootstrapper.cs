using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.Platform.Persistence;

/// <summary>
/// Creates the physical schema of every module.
/// <para>
/// Each module owns its own tables and creates them itself, so the schema of
/// the system is the sum of the schemas of the modules — there is no central
/// migration project. The creation script of EF Core is rewritten to
/// <c>IF NOT EXISTS</c> so that booting the application is idempotent and
/// every module can safely (re)create its own tables.
/// </para>
/// </summary>
public sealed class SchemaBootstrapper
{
    private readonly IEnumerable<ModuleDbContext> _contexts;

    public SchemaBootstrapper(IEnumerable<ModuleDbContext> contexts) => _contexts = contexts;

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        foreach (var context in _contexts)
        {
            var script = context.Database.GenerateCreateScript();

            foreach (var statement in SchemaScript.Split(script))
            {
                await context.Database
                    .ExecuteSqlRawAsync(statement, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}

/// <summary>Helpers to turn an EF Core create script into an idempotent one.</summary>
public static class SchemaScript
{
    public static IEnumerable<string> Split(string script) =>
        script.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(statement => statement.Trim())
            .Where(statement => statement.Length > 0)
            .Select(MakeIdempotent);

    private static string MakeIdempotent(string statement)
    {
        if (statement.StartsWith("CREATE TABLE ", StringComparison.Ordinal))
            return "CREATE TABLE IF NOT EXISTS " + statement["CREATE TABLE ".Length..];

        if (statement.StartsWith("CREATE UNIQUE INDEX ", StringComparison.Ordinal))
            return "CREATE UNIQUE INDEX IF NOT EXISTS " + statement["CREATE UNIQUE INDEX ".Length..];

        if (statement.StartsWith("CREATE INDEX ", StringComparison.Ordinal))
            return "CREATE INDEX IF NOT EXISTS " + statement["CREATE INDEX ".Length..];

        return statement;
    }
}
