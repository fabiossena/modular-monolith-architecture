using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ModularMonolith.Modules.Cart.Contracts;

namespace ModularMonolith.Modules.Cart.Api;

/// <summary>
/// The HTTP surface of the Cart module. A cart belongs to the browser that
/// created it, so it is addressed by its own id.
/// </summary>
public static class CartEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var cart = app.MapGroup("/api/cart/{cartId}");

        cart.MapGet("", async (
            ICartService service,
            string cartId,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAsync(cartId, cancellationToken)));

        cart.MapPost("/items", async (
            ICartService service,
            string cartId,
            AddCartItemRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await service.AddAsync(cartId, request.ProductId, request.Quantity, cancellationToken));
            }
            catch (KeyNotFoundException exception)
            {
                return Results.NotFound(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }
        });

        cart.MapPut("/items/{productId:int}", async (
            ICartService service,
            string cartId,
            int productId,
            SetQuantityRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await service.SetQuantityAsync(cartId, productId, request.Quantity, cancellationToken));
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }
        });

        cart.MapDelete("/items/{productId:int}", async (
            ICartService service,
            string cartId,
            int productId,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.RemoveAsync(cartId, productId, cancellationToken)));

        cart.MapDelete("", async (
            ICartService service,
            string cartId,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ClearAsync(cartId, cancellationToken)));

        return app;
    }
}

public record AddCartItemRequest(int ProductId, int Quantity);

public record SetQuantityRequest(int Quantity);
