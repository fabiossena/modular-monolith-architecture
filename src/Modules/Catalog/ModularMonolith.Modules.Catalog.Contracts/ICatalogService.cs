namespace ModularMonolith.Modules.Catalog.Contracts;

/// <summary>
/// The public surface of the Catalog bounded context. Other modules may only
/// depend on this interface — never on the implementation, never on the table.
/// </summary>
public interface ICatalogService
{
    Task<IReadOnlyList<ProductDto>> SearchAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken = default);

    Task<ProductDto?> GetAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductDto>> GetManyAsync(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> CategoriesAsync(CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default);
}
