using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ModularMonolith.Modules.Inventory.Contracts;
using ModularMonolith.Shared.Platform.Security;

namespace ModularMonolith.Modules.Inventory.Api;

/// <summary>
/// The HTTP surface of the Inventory module. Reading the stock is public —
/// the store shows the availability on every product — while changing it is
/// reserved for an administrator.
/// </summary>
public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var inventory = app.MapGroup("/api/inventory");

        inventory.MapGet("", async (
            IInventoryService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAllAsync(cancellationToken)));

        inventory.MapPut("/{productId:int}/quantity", async (
            IInventoryService service,
            int productId,
            AdjustQuantityRequest request,
            CancellationToken cancellationToken) =>
        {
            var item = await service.AdjustAsync(productId, request.Quantity, request.ReorderLevel, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).RequireAuthorization(AuthorizationPolicyNames.Admin);

        inventory.MapPost("/{productId:int}/restock", async (
            IInventoryService service,
            int productId,
            RestockRequest request,
            CancellationToken cancellationToken) =>
        {
            var item = await service.AddStockAsync(productId, request.Delta, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).RequireAuthorization(AuthorizationPolicyNames.Admin);

        return app;
    }
}

public record AdjustQuantityRequest(int Quantity, int? ReorderLevel);

public record RestockRequest(int Delta);
