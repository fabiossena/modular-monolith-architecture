using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Shared.Kernel;
using ModularMonolith.Shared.Platform.Security;

namespace ModularMonolith.Modules.Catalog.Api;

/// <summary>
/// The HTTP surface of the Catalog module. The endpoints are thin: they
/// translate a request into a call on the application service and translate
/// the answer back into a response. No business rule lives here.
/// </summary>
public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var catalog = app.MapGroup("/api/catalog");

        catalog.MapGet("/products", async (
            ICatalogService service,
            string? q,
            string? category,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.SearchAsync(q, category, cancellationToken)));

        catalog.MapGet("/categories", async (
            ICatalogService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CategoriesAsync(cancellationToken)));

        catalog.MapGet("/products/{id:int}", async (
            ICatalogService service,
            int id,
            CancellationToken cancellationToken) =>
        {
            var product = await service.GetAsync(id, cancellationToken);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        // "Add a production" — an administrator registers a new product.
        catalog.MapPost("/products", async (
            ICatalogService service,
            CreateProductRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var product = await service.CreateAsync(
                    new CreateProductCommand(
                        request.Sku,
                        request.Name,
                        request.Description,
                        request.Category,
                        request.Emoji,
                        request.Price,
                        request.InitialStock,
                        request.ReorderLevel),
                    cancellationToken);

                return Results.Ok(product);
            }
            catch (DomainException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }
        }).RequireAuthorization(AuthorizationPolicyNames.Admin);

        return app;
    }
}

/// <summary>Payload of the "add a production" endpoint.</summary>
public record CreateProductRequest(
    string Sku,
    string Name,
    string Description,
    string Category,
    string Emoji,
    decimal Price,
    int InitialStock,
    int ReorderLevel);
