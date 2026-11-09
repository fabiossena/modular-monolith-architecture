using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Modules.Identity.Api;
using ModularMonolith.Modules.Identity.Application;
using ModularMonolith.Modules.Identity.Application.Abstractions;
using ModularMonolith.Modules.Identity.Contracts;
using ModularMonolith.Modules.Identity.Infrastructure;
using ModularMonolith.Shared.Platform.Persistence;
using ModularMonolith.Shared.Platform.Seeding;

namespace ModularMonolith.Modules.Identity;

/// <summary>Composition of the Identity module.</summary>
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddDbContext<IdentityDbContext>((provider, options) =>
            options.UseSqlite(provider.GetRequiredService<SqliteConnectionProvider>().Connection));

        services.AddScoped<ModuleDbContext>(provider => provider.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IUserStore, UserStore>();
        services.AddScoped<IIdentityService, IdentityApplicationService>();
        services.AddScoped<PasswordHasher>();
        services.AddScoped<IdentitySeeder>();
        services.AddScoped<IModuleSeeder>(provider => provider.GetRequiredService<IdentitySeeder>());

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder app) =>
        app.MapIdentityEndpoints();
}
