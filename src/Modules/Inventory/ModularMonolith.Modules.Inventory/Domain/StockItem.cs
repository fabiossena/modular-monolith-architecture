using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Inventory.Domain;

/// <summary>
/// The Inventory aggregate: how many units of a product are available. It is
/// the only aggregate in the system allowed to change a stock quantity.
/// </summary>
public sealed class StockItem : AggregateRoot<int>
{
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int ReorderLevel { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private StockItem()
    {
        // Required by EF Core.
    }

    public StockItem(int productId, int quantity, int reorderLevel)
    {
        if (productId <= 0)
            throw new DomainException("The product id is required.");

        ProductId = productId;
        Quantity = Math.Max(0, quantity);
        ReorderLevel = Math.Max(0, reorderLevel);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Takes the requested units out of the stock. Returns false — and changes
    /// nothing — when there are not enough units.
    /// </summary>
    public bool Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("The reserved quantity must be positive.");

        if (Quantity < quantity)
            return false;

        Quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>Sets an absolute quantity, as an administrator does.</summary>
    public void AdjustTo(int quantity, int? reorderLevel)
    {
        Quantity = Math.Max(0, quantity);
        if (reorderLevel is not null)
            ReorderLevel = Math.Max(0, reorderLevel.Value);

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Adds (or removes) units, as a restock does.</summary>
    public void AddStock(int delta)
    {
        Quantity = Math.Max(0, Quantity + delta);
        UpdatedAt = DateTime.UtcNow;
    }
}
