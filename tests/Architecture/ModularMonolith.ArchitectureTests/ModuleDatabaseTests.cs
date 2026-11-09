using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Platform.Persistence;
using Xunit;

namespace ModularMonolith.ArchitectureTests;

/// <summary>
/// The modules share one physical database, but not one schema. This test
/// proves that the tables are really independent: no module may point to the
/// table of another module with a foreign key.
/// </summary>
public class ModuleDatabaseTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"modular-monolith-architecture-{Guid.NewGuid():N}.db");

    private readonly SqliteConnectionProvider _connectionProvider;
    private readonly TransactionCoordinator _transactionCoordinator;
    private readonly List<DbContext> _contexts = [];

    public ModuleDatabaseTests()
    {
        _connectionProvider = new SqliteConnectionProvider($"Data Source={_databasePath}");
        _transactionCoordinator = new TransactionCoordinator(_connectionProvider);
    }

    [Fact]
    public void No_module_table_points_to_the_table_of_another_module()
    {
        foreach (var context in CreateContexts())
        {
            var owner = ModuleOf(context.GetType());

            foreach (var entityType in context.Model.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    var principal = foreignKey.PrincipalEntityType.ClrType;

                    Assert.Equal(
                        owner,
                        ModuleOf(principal));
                }
            }
        }
    }

    [Fact]
    public void Every_module_owns_its_own_tables()
    {
        var owners = CreateContexts()
            .SelectMany(context => context.Model.GetEntityTypes())
            .Select(entityType => ModuleOf(entityType.ClrType))
            .Distinct()
            .OrderBy(module => module, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            Modules.All.Select(module => module.Name).OrderBy(name => name, StringComparer.Ordinal),
            owners);
    }

    [Fact]
    public void The_tables_of_two_modules_never_share_a_name()
    {
        var tables = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var context in CreateContexts())
        {
            var owner = ModuleOf(context.GetType());

            foreach (var entityType in context.Model.GetEntityTypes())
            {
                var table = entityType.GetTableName();

                if (table is null)
                    continue;

                Assert.False(
                    tables.TryGetValue(table, out var previousOwner) && previousOwner != owner,
                    $"The table '{table}' is declared by both '{previousOwner}' and '{owner}'.");

                tables[table] = owner;
            }
        }
    }

    private IEnumerable<DbContext> CreateContexts()
    {
        var options = _connectionProvider.Connection;

        var contexts = new DbContext[]
        {
            new ModularMonolith.Modules.Catalog.Infrastructure.CatalogDbContext(
                new DbContextOptionsBuilder<ModularMonolith.Modules.Catalog.Infrastructure.CatalogDbContext>()
                    .UseSqlite(options).Options,
                _transactionCoordinator),

            new ModularMonolith.Modules.Inventory.Infrastructure.InventoryDbContext(
                new DbContextOptionsBuilder<ModularMonolith.Modules.Inventory.Infrastructure.InventoryDbContext>()
                    .UseSqlite(options).Options,
                _transactionCoordinator),

            new ModularMonolith.Modules.Cart.Infrastructure.CartDbContext(
                new DbContextOptionsBuilder<ModularMonolith.Modules.Cart.Infrastructure.CartDbContext>()
                    .UseSqlite(options).Options,
                _transactionCoordinator),

            new ModularMonolith.Modules.Payments.Infrastructure.PaymentsDbContext(
                new DbContextOptionsBuilder<ModularMonolith.Modules.Payments.Infrastructure.PaymentsDbContext>()
                    .UseSqlite(options).Options,
                _transactionCoordinator),

            new ModularMonolith.Modules.Orders.Infrastructure.OrdersDbContext(
                new DbContextOptionsBuilder<ModularMonolith.Modules.Orders.Infrastructure.OrdersDbContext>()
                    .UseSqlite(options).Options,
                _transactionCoordinator),

            new ModularMonolith.Modules.Identity.Infrastructure.IdentityDbContext(
                new DbContextOptionsBuilder<ModularMonolith.Modules.Identity.Infrastructure.IdentityDbContext>()
                    .UseSqlite(options).Options,
                _transactionCoordinator),
        };

        _contexts.AddRange(contexts);

        return contexts;
    }

    private static string ModuleOf(Type type)
    {
        var segments = type.Namespace?.Split('.') ?? [];
        var index = Array.IndexOf(segments, "Modules");

        Assert.True(index >= 0 && index + 1 < segments.Length, $"'{type.FullName}' does not belong to a module.");

        return segments[index + 1];
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
            context.Dispose();

        _connectionProvider.Dispose();

        foreach (var suffix in new[] { "", "-shm", "-wal" })
        {
            try
            {
                File.Delete(_databasePath + suffix);
            }
            catch (IOException)
            {
                // The file is still held by the operating system; the temporary
                // folder takes care of it.
            }
        }
    }
}
