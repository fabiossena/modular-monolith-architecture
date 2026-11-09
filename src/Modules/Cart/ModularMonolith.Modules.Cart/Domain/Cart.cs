using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Cart.Domain;

/// <summary>
/// The Cart aggregate. It stores the product id and the quantity only: the
/// name, the price and the emoji of a product are always read live from the
/// Catalog module, so a cart can never hold a stale price.
/// </summary>
public sealed class Cart : AggregateRoot<string>
{
    public DateTime CreatedAt { get; private set; }

    public List<CartLine> Items { get; private set; } = [];

    private Cart()
    {
        // Required by EF Core.
    }

    public Cart(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("The cart id is required.");

        Id = id.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void AddLine(int productId, int quantity)
    {
        if (productId <= 0)
            throw new DomainException("The product id is required.");

        var line = Items.FirstOrDefault(i => i.ProductId == productId);

        if (line is null)
            Items.Add(new CartLine(Id, productId, quantity));
        else
            line.Increase(quantity);
    }

    public void SetLineQuantity(int productId, int quantity)
    {
        var line = Items.FirstOrDefault(i => i.ProductId == productId);
        if (line is null)
            return;

        if (quantity <= 0)
            Items.Remove(line);
        else
            line.SetQuantity(quantity);
    }

    public void RemoveLine(int productId)
    {
        var line = Items.FirstOrDefault(i => i.ProductId == productId);
        if (line is not null)
            Items.Remove(line);
    }

    public void Clear() => Items.Clear();
}
