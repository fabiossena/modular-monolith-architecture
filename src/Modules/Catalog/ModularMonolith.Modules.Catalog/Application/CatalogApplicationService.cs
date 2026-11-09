using ModularMonolith.Modules.Catalog.Application.Abstractions;
using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Modules.Catalog.Domain;
using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Catalog.Application;

/// <summary>
/// The application service of the Catalog context. It is the only place where
/// the catalog use cases live; other modules reach it through
/// <see cref="ICatalogService"/>.
/// </summary>
public sealed class CatalogApplicationService(
    IProductStore products,
    IDomainEventPublisher domainEventPublisher) : ICatalogService
{
    public async Task<IReadOnlyList<ProductDto>> SearchAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken = default)
    {
        var found = await products.SearchAsync(search, category, cancellationToken);
        return found.Select(ToDto).ToList();
    }

    public async Task<ProductDto?> GetAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await products.FindAsync(productId, cancellationToken);
        return product is null ? null : ToDto(product);
    }

    public async Task<IReadOnlyList<ProductDto>> GetManyAsync(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        var found = await products.FindByIdsAsync(productIds.Distinct().ToList(), cancellationToken);
        return found.Select(ToDto).ToList();
    }

    public Task<IReadOnlyList<string>> CategoriesAsync(CancellationToken cancellationToken = default) =>
        products.CategoriesAsync(cancellationToken);

    public async Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var sku = Sku.Create(command.Sku);

        if (await products.SkuExistsAsync(sku.Value, cancellationToken))
            throw new DomainException($"There is already a product with the SKU '{sku.Value}'.");

        var product = new Product(
            sku,
            command.Name,
            command.Description,
            command.Category,
            command.Emoji,
            Money.From(command.Price));

        products.Add(product);
        await products.SaveChangesAsync(cancellationToken);

        // The product now has its identifier: the Inventory module is told to
        // open the stock record through the domain event.
        product.AnnounceCreated(command.InitialStock, command.ReorderLevel);
        await domainEventPublisher.PublishAsync(product.PullDomainEvents(), cancellationToken);

        return ToDto(product);
    }

    private static ProductDto ToDto(Product product) =>
        new(
            product.Id,
            product.Sku,
            product.Name,
            product.Description,
            product.Category,
            product.Emoji,
            product.Price,
            product.Active);
}
