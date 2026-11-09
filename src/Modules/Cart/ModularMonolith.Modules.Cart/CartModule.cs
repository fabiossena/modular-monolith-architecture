using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Modules.Cart.Api;
using ModularMonolith.Modules.Cart.Application;
using ModularMonolith.Modules.Cart.Application.Abstractions;
using ModularMonolith.Modules.Cart.Contracts;
using ModularMonolith.Modules.Cart.Infrastructure;
using ModularMonolith.Shared.Platform.Persistence;

namespace ModularMonolith.Modules.Cart;

/// <summary>Composition of the Cart module.</summary>
public static class CartModule
{
    public static IServiceCollection AddCartModule(this IServiceCollection services)
    {
        services.AddDbContext<CartDbContext>((provider, options) =>
            options.UseSqlite(provider.GetRequiredService<SqliteConnectionProvider>().Connection));

        services.AddScoped<ModuleDbContext>(provider => provider.GetRequiredService<CartDbContext>());
        services.AddScoped<ICartStore, CartStore>();
        services.AddScoped<ICartService, CartApplicationService>();

        return services;
    }

    public static IEndpointRouteBuilder MapCartModule(this IEndpointRouteBuilder app) =>
        app.MapCartEndpoints();
}
