using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ModularMonolith.Modules.Payments.Contracts;

namespace ModularMonolith.Modules.Payments.Api;

/// <summary>The HTTP surface of the Payments module.</summary>
public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var payments = app.MapGroup("/api/payments");

        payments.MapGet("/methods", (IPaymentService service) =>
            Results.Ok(service.SupportedMethods));

        return app;
    }
}
