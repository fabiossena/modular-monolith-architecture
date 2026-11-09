using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Modules.Orders.Api;
using ModularMonolith.Modules.Orders.Application;
using ModularMonolith.Modules.Orders.Application.Abstractions;
using ModularMonolith.Modules.Orders.Contracts;
using ModularMonolith.Modules.Orders.Infrastructure;
using ModularMonolith.Shared.Platform.Persistence;

namespace ModularMonolith.Modules.Orders;

/// <summary>Composition of the Orders module.</summary>
public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        services.AddDbContext<OrdersDbContext>((provider, options) =>
            options.UseSqlite(provider.GetRequiredService<SqliteConnectionProvider>().Connection));

        services.AddScoped<ModuleDbContext>(provider => provider.GetRequiredService<OrdersDbContext>());
        services.AddScoped<IOrderStore, OrderStore>();
        services.AddScoped<IOrderService, OrdersApplicationService>();

        return services;
    }

    public static IEndpointRouteBuilder MapOrdersModule(this IEndpointRouteBuilder app) =>
        app.MapOrderEndpoints();
}
