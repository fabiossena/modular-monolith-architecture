using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Catalog.Domain;

/// <summary>The stock keeping unit of a product. A value object: it has identity
/// only through its own value.</summary>
public sealed class Sku : ValueObject
{
    public string Value { get; }

    private Sku(string value) => Value = value;

    public static Sku Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToUpperInvariant();

        if (normalized.Length == 0)
            throw new DomainException("The SKU is required.");

        if (normalized.Length > 32)
            throw new DomainException("The SKU must have at most 32 characters.");

        return new Sku(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
