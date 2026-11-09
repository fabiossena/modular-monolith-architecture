using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ModularMonolith.Modules.Orders.Contracts;
using ModularMonolith.Shared.Platform.Security;

namespace ModularMonolith.Modules.Orders.Api;

/// <summary>
/// The HTTP surface of the Orders module.
/// <list type="bullet">
/// <item>the list of every order is the administration view of "what was bought";</item>
/// <item>the list of the orders of the signed in buyer is the buyer view;</item>
/// <item>the checkout finalizes an order.</item>
/// </list>
/// </summary>
public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/orders");

        // Administration: every purchase of the store.
        orders.MapGet("", async (
            IOrderService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(cancellationToken)))
            .RequireAuthorization(AuthorizationPolicyNames.Admin);

        // Buyer: the orders of the signed in account.
        orders.MapGet("/mine", async (
            IOrderService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value
                        ?? user.FindFirst("email")?.Value;

            if (string.IsNullOrWhiteSpace(email))
                return Results.Unauthorized();

            return Results.Ok(await service.ListByEmailAsync(email, cancellationToken));
        }).RequireAuthorization(AuthorizationPolicyNames.Buyer);

        orders.MapGet("/{id:int}", async (
            IOrderService service,
            int id,
            CancellationToken cancellationToken) =>
        {
            var order = await service.GetAsync(id, cancellationToken);
            return order is null ? Results.NotFound() : Results.Ok(order);
        }).RequireAuthorization();

        orders.MapPost("/checkout", async (
            IOrderService service,
            CheckoutRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await service.CheckoutAsync(
                new CheckoutCommand(
                    request.CartId,
                    request.CustomerName,
                    request.Email,
                    request.Address,
                    request.City,
                    request.Zip,
                    request.PaymentMethod,
                    request.SimulateFailure),
                cancellationToken);

            return result.Ok
                ? Results.Ok(result.Order)
                : Results.BadRequest(new { error = result.Error });
        }).RequireAuthorization(AuthorizationPolicyNames.Buyer);

        return app;
    }
}

public record CheckoutRequest(
    string CartId,
    string CustomerName,
    string Email,
    string Address,
    string City,
    string Zip,
    string PaymentMethod,
    bool SimulateFailure);
