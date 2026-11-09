namespace ModularMonolith.Modules.Cart.Contracts;

/// <summary>
/// The public surface of the Cart bounded context. The cart stores only the
/// product id and the quantity — product data is always read live from the
/// Catalog module.
/// </summary>
public interface ICartService
{
    Task<CartView> GetAsync(string cartId, CancellationToken cancellationToken = default);

    Task<CartView> AddAsync(
        string cartId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<CartView> SetQuantityAsync(
        string cartId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<CartView> RemoveAsync(
        string cartId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<CartView> ClearAsync(string cartId, CancellationToken cancellationToken = default);
}
