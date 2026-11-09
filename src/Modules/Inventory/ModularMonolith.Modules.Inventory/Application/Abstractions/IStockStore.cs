using ModularMonolith.Modules.Inventory.Domain;

namespace ModularMonolith.Modules.Inventory.Application.Abstractions;

/// <summary>
/// The port the Inventory application layer uses to reach its own storage.
/// </summary>
public interface IStockStore
{
    Task<IReadOnlyList<StockItem>> ListAsync(CancellationToken cancellationToken = default);

    Task<StockItem?> FindByProductAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockItem>> FindByProductIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);

    void Add(StockItem stockItem);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
