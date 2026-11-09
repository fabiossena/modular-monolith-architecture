namespace ModularMonolith.Modules.Inventory.Contracts;

/// <summary>Stock information of a product, as seen by the other modules.</summary>
public record InventoryItemDto(
    int ProductId,
    string ProductName,
    string Sku,
    string Emoji,
    int Quantity,
    int ReorderLevel,
    DateTime UpdatedAt);
