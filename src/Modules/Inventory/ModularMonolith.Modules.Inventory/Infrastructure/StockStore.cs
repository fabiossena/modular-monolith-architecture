using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Inventory.Application.Abstractions;
using ModularMonolith.Modules.Inventory.Domain;

namespace ModularMonolith.Modules.Inventory.Infrastructure;

/// <summary>
/// The Inventory storage. It is the only class of the module that touches
/// the stock table.
/// </summary>
public sealed class StockStore(InventoryDbContext db) : IStockStore
{
    public async Task<IReadOnlyList<StockItem>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.StockItems.AsNoTracking()
            .OrderBy(i => i.ProductId)
            .ToListAsync(cancellationToken);

    public Task<StockItem?> FindByProductAsync(int productId, CancellationToken cancellationToken = default) =>
        db.StockItems.FirstOrDefaultAsync(i => i.ProductId == productId, cancellationToken);

    public async Task<IReadOnlyList<StockItem>> FindByProductIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return [];

        var ids = productIds.Distinct().ToList();

        return await db.StockItems.AsNoTracking()
            .Where(i => ids.Contains(i.ProductId))
            .ToListAsync(cancellationToken);
    }

    public void Add(StockItem stockItem) => db.StockItems.Add(stockItem);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
