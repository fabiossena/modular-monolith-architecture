namespace ModularMonolith.Modules.Cart.Contracts;

/// <summary>A line of the shopping cart.</summary>
public record CartLineView(
    int ProductId,
    string Name,
    string Emoji,
    string Sku,
    decimal Price,
    int Quantity,
    int Stock,
    decimal Subtotal);

/// <summary>The shopping cart of a single visitor.</summary>
public record CartView(
    string Id,
    IReadOnlyList<CartLineView> Items,
    decimal Subtotal,
    int TotalItems);
