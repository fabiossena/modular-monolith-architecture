namespace ModularMonolith.Modules.Inventory.Contracts;

/// <summary>
/// The public surface of the Inventory bounded context. It is the only writer
/// of stock quantities: every other module asks it, never updates a row.
/// </summary>
public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<int> GetQuantityAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, int>> GetQuantitiesAsync(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);

    /// <summary>Decrements the stock. Returns false when there is not enough.</summary>
    Task<bool> ReserveAsync(int productId, int quantity, CancellationToken cancellationToken = default);

    Task<InventoryItemDto?> AdjustAsync(
        int productId,
        int quantity,
        int? reorderLevel,
        CancellationToken cancellationToken = default);

    Task<InventoryItemDto?> AddStockAsync(
        int productId,
        int delta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the stock record of a product when it does not exist yet.
    /// Returns the existing record otherwise.
    /// </summary>
    Task<InventoryItemDto> EnsureAsync(
        int productId,
        int initialQuantity,
        int reorderLevel,
        CancellationToken cancellationToken = default);
}
