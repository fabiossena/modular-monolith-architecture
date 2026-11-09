using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Catalog.Domain;

/// <summary>
/// The Catalog aggregate: what the store sells and for how much. It knows
/// nothing about stock — that belongs to the Inventory context.
/// </summary>
public sealed class Product : AggregateRoot<int>
{
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Emoji { get; private set; } = "📦";
    public decimal Price { get; private set; }
    public bool Active { get; private set; } = true;

    private Product()
    {
        // Required by EF Core.
    }

    public Product(Sku sku, string name, string description, string category, string emoji, Money price)
    {
        Sku = sku.Value;
        Name = Required(name, "name");
        Description = (description ?? string.Empty).Trim();
        Category = Required(category, "category");
        Emoji = string.IsNullOrWhiteSpace(emoji) ? "📦" : emoji.Trim();
        Price = price.Amount;
        Active = true;
    }

    /// <summary>
    /// Announces that the product exists. Called by the application layer
    /// right after the aggregate was persisted, because only then the
    /// identifier of the product is known.
    /// </summary>
    public void AnnounceCreated(int initialStock, int reorderLevel) =>
        RaiseDomainEvent(new ProductCreated(Id, Sku, Name, initialStock, reorderLevel));

    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"The product {field} is required.");

        return value.Trim();
    }
}
