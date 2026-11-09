using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Modules.Catalog.Api;
using ModularMonolith.Modules.Catalog.Application;
using ModularMonolith.Modules.Catalog.Application.Abstractions;
using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Modules.Catalog.Infrastructure;
using ModularMonolith.Shared.Platform.Persistence;
using ModularMonolith.Shared.Platform.Seeding;

namespace ModularMonolith.Modules.Catalog;

/// <summary>
/// Composition of the Catalog module. The host calls this extension and
/// nothing else: the module registers its own context, its own storage, its
/// own application service and its own endpoints.
/// </summary>
public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        services.AddDbContext<CatalogDbContext>((provider, options) =>
            options.UseSqlite(provider.GetRequiredService<SqliteConnectionProvider>().Connection));

        services.AddScoped<ModuleDbContext>(provider => provider.GetRequiredService<CatalogDbContext>());
        services.AddScoped<IProductStore, ProductStore>();
        services.AddScoped<ICatalogService, CatalogApplicationService>();
        services.AddScoped<CatalogSeeder>();
        services.AddScoped<IModuleSeeder>(provider => provider.GetRequiredService<CatalogSeeder>());

        return services;
    }

    public static IEndpointRouteBuilder MapCatalogModule(this IEndpointRouteBuilder app) =>
        app.MapCatalogEndpoints();
}
