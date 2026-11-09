using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Modules.Payments.Api;
using ModularMonolith.Modules.Payments.Application;
using ModularMonolith.Modules.Payments.Application.Abstractions;
using ModularMonolith.Modules.Payments.Contracts;
using ModularMonolith.Modules.Payments.Infrastructure;
using ModularMonolith.Shared.Platform.Persistence;

namespace ModularMonolith.Modules.Payments;

/// <summary>Composition of the Payments module.</summary>
public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
    {
        services.AddDbContext<PaymentsDbContext>((provider, options) =>
            options.UseSqlite(provider.GetRequiredService<SqliteConnectionProvider>().Connection));

        services.AddScoped<ModuleDbContext>(provider => provider.GetRequiredService<PaymentsDbContext>());
        services.AddScoped<IPaymentStore, PaymentStore>();
        services.AddScoped<IPaymentService, SimulatedPaymentService>();

        return services;
    }

    public static IEndpointRouteBuilder MapPaymentsModule(this IEndpointRouteBuilder app) =>
        app.MapPaymentEndpoints();
}
