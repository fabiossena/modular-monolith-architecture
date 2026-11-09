using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Modules.Inventory.Application.Abstractions;
using ModularMonolith.Modules.Inventory.Contracts;
using ModularMonolith.Modules.Inventory.Domain;

namespace ModularMonolith.Modules.Inventory.Application;

/// <summary>
/// The application service of the Inventory context. It is the only writer of
/// stock quantities; the product description it returns comes from the
/// Catalog module, read through its public contract.
/// </summary>
public sealed class InventoryApplicationService(
    IStockStore stock,
    ICatalogService catalog) : IInventoryService
{
    public async Task<IReadOnlyList<InventoryItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await stock.ListAsync(cancellationToken);
        return await ToDtoAsync(items, cancellationToken);
    }

    public async Task<int> GetQuantityAsync(int productId, CancellationToken cancellationToken = default)
    {
        var item = await stock.FindByProductAsync(productId, cancellationToken);
        return item?.Quantity ?? 0;
    }

    public async Task<IReadOnlyDictionary<int, int>> GetQuantitiesAsync(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        var items = await stock.FindByProductIdsAsync(productIds.Distinct().ToList(), cancellationToken);
        return items.ToDictionary(i => i.ProductId, i => i.Quantity);
    }

    public async Task<bool> ReserveAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var item = await stock.FindByProductAsync(productId, cancellationToken);
        if (item is null || !item.Reserve(quantity))
            return false;

        await stock.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<InventoryItemDto?> AdjustAsync(
        int productId,
        int quantity,
        int? reorderLevel,
        CancellationToken cancellationToken = default)
    {
        var item = await stock.FindByProductAsync(productId, cancellationToken);
        if (item is null)
            return null;

        item.AdjustTo(quantity, reorderLevel);
        await stock.SaveChangesAsync(cancellationToken);

        return (await ToDtoAsync([item], cancellationToken))[0];
    }

    public async Task<InventoryItemDto?> AddStockAsync(
        int productId,
        int delta,
        CancellationToken cancellationToken = default)
    {
        var item = await stock.FindByProductAsync(productId, cancellationToken);
        if (item is null)
            return null;

        item.AddStock(delta);
        await stock.SaveChangesAsync(cancellationToken);

        return (await ToDtoAsync([item], cancellationToken))[0];
    }

    public async Task<InventoryItemDto> EnsureAsync(
        int productId,
        int initialQuantity,
        int reorderLevel,
        CancellationToken cancellationToken = default)
    {
        var item = await stock.FindByProductAsync(productId, cancellationToken);

        if (item is null)
        {
            item = new StockItem(productId, initialQuantity, reorderLevel);
            stock.Add(item);
            await stock.SaveChangesAsync(cancellationToken);
        }

        return (await ToDtoAsync([item], cancellationToken))[0];
    }

    private async Task<IReadOnlyList<InventoryItemDto>> ToDtoAsync(
        IReadOnlyCollection<StockItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return [];

        var products = await catalog.GetManyAsync(items.Select(i => i.ProductId), cancellationToken);
        var byId = products.ToDictionary(p => p.Id);

        return items
            .Select(item =>
            {
                byId.TryGetValue(item.ProductId, out var product);
                return new InventoryItemDto(
                    item.ProductId,
                    product?.Name ?? $"Product #{item.ProductId}",
                    product?.Sku ?? string.Empty,
                    product?.Emoji ?? "📦",
                    item.Quantity,
                    item.ReorderLevel,
                    item.UpdatedAt);
            })
            .ToList();
    }
}
