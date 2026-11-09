using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Modules.Inventory.Api;
using ModularMonolith.Modules.Inventory.Application;
using ModularMonolith.Modules.Inventory.Application.Abstractions;
using ModularMonolith.Modules.Inventory.Contracts;
using ModularMonolith.Modules.Inventory.Infrastructure;
using ModularMonolith.Shared.Kernel;
using ModularMonolith.Shared.Platform.Persistence;
using ModularMonolith.Shared.Platform.Seeding;

namespace ModularMonolith.Modules.Inventory;

/// <summary>Composition of the Inventory module.</summary>
public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddDbContext<InventoryDbContext>((provider, options) =>
            options.UseSqlite(provider.GetRequiredService<SqliteConnectionProvider>().Connection));

        services.AddScoped<ModuleDbContext>(provider => provider.GetRequiredService<InventoryDbContext>());
        services.AddScoped<IStockStore, StockStore>();
        services.AddScoped<IInventoryService, InventoryApplicationService>();
        services.AddScoped<InventorySeeder>();
        services.AddScoped<IModuleSeeder>(provider => provider.GetRequiredService<InventorySeeder>());
        services.AddScoped<IDomainEventHandler, ProductCreatedHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapInventoryModule(this IEndpointRouteBuilder app) =>
        app.MapInventoryEndpoints();
}
