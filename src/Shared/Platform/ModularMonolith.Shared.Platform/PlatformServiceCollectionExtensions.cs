using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Kernel;
using ModularMonolith.Shared.Platform.Events;
using ModularMonolith.Shared.Platform.Persistence;
using ModularMonolith.Shared.Platform.Security;

namespace ModularMonolith.Shared.Platform;

/// <summary>
/// Registers the technical plumbing that every module shares: the single
/// SQLite connection, the transaction coordinator, the in-process event
/// publisher and the current-user accessor. It contains no business rule.
/// </summary>
public static class PlatformServiceCollectionExtensions
{
    public static IServiceCollection AddModularMonolithPlatform(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped(_ => new SqliteConnectionProvider(connectionString));
        services.AddScoped<TransactionCoordinator>();
        services.AddScoped<SchemaBootstrapper>();
        services.AddScoped<IDomainEventPublisher, InProcessDomainEventPublisher>();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddHttpContextAccessor();

        return services;
    }
}
