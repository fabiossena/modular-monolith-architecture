using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Orders.Domain;

/// <summary>A product line of an order.</summary>
public sealed class OrderItem : Entity<int>
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Emoji { get; private set; } = "📦";
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem()
    {
        // Required by EF Core.
    }

    public OrderItem(int productId, string name, string sku, string emoji, decimal unitPrice, int quantity)
    {
        if (productId <= 0)
            throw new DomainException("The product id is required.");

        if (quantity <= 0)
            throw new DomainException("The quantity must be positive.");

        ProductId = productId;
        Name = name ?? string.Empty;
        Sku = sku ?? string.Empty;
        Emoji = string.IsNullOrWhiteSpace(emoji) ? "📦" : emoji;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
