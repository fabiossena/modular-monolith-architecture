using ModularMonolith.Modules.Catalog.Domain;

namespace ModularMonolith.Modules.Catalog.Application.Abstractions;

/// <summary>
/// The port the Catalog application layer uses to reach its own storage. The
/// implementation lives in the Infrastructure layer and is the only place
/// that knows about the Catalog tables.
/// </summary>
public interface IProductStore
{
    Task<IReadOnlyList<Product>> SearchAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken = default);

    Task<Product?> FindAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> FindByIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> CategoriesAsync(CancellationToken cancellationToken = default);

    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);

    void Add(Product product);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
