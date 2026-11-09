using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Cart.Domain;

/// <summary>A product inside a cart.</summary>
public sealed class CartLine : Entity<int>
{
    public string CartId { get; private set; } = string.Empty;
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }

    private CartLine()
    {
        // Required by EF Core.
    }

    public CartLine(string cartId, int productId, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("The quantity must be positive.");

        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    public void Increase(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("The quantity must be positive.");

        Quantity += quantity;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("The quantity must be positive.");

        Quantity = quantity;
    }
}
