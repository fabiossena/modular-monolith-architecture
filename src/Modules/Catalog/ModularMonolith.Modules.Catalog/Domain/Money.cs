using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Catalog.Domain;

/// <summary>A monetary amount. Kept as a value object so a price can never be
/// compared with a quantity or stored with more than two decimals.</summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Money From(decimal amount)
    {
        if (amount < 0m)
            throw new DomainException("The price cannot be negative.");

        return new Money(decimal.Round(amount, 2, MidpointRounding.ToEven));
    }

    public static Money Zero => new(0m);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString() => Amount.ToString("F2");
}
