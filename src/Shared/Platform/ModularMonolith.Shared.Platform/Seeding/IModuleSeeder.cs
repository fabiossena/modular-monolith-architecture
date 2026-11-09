namespace ModularMonolith.Shared.Platform.Seeding;

/// <summary>
/// A module that contributes reference data to the database on first boot.
/// <para>
/// Seeders are ordered: a module that depends on the data of another module
/// declares a higher <see cref="Order"/>.
/// </para>
/// </summary>
public interface IModuleSeeder
{
    /// <summary>Execution order; lower values run first.</summary>
    int Order { get; }

    Task SeedAsync(CancellationToken cancellationToken = default);
}
