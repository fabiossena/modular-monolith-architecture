using ModularMonolith.Modules.Orders.Contracts;
using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Orders.Domain;

/// <summary>
/// The Order aggregate: what a buyer bought. The order keeps a copy of the
/// product name, sku and price of the moment of the purchase — an order is a
/// historical document and must not change when the catalog changes.
/// </summary>
public sealed class Order : AggregateRoot<int>
{
    public string Code { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Zip { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public List<OrderItem> Items { get; private set; } = [];

    private Order()
    {
        // Required by EF Core.
    }

    public Order(
        string customerName,
        string email,
        string address,
        string city,
        string zip,
        IEnumerable<OrderLine> lines)
    {
        CustomerName = Required(customerName, "name");
        Email = Required(email, "e-mail");
        Address = (address ?? string.Empty).Trim();
        City = (city ?? string.Empty).Trim();
        Zip = (zip ?? string.Empty).Trim();
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        foreach (var line in lines)
            Items.Add(new OrderItem(line.ProductId, line.Name, line.Sku, line.Emoji, line.UnitPrice, line.Quantity));

        Total = Items.Sum(i => i.UnitPrice * i.Quantity);
    }

    /// <summary>The human readable code of the order, known after it was saved.</summary>
    public void AssignCode(int id) => Code = $"ORD-{id:D5}";

    public void MarkPaid()
    {
        if (Status is OrderStatus.Pending)
            Status = OrderStatus.Paid;
    }

    public void MarkCancelled()
    {
        if (Status is OrderStatus.Pending or OrderStatus.Paid)
            Status = OrderStatus.Cancelled;
    }

    public void MarkShipped()
    {
        if (Status is OrderStatus.Paid)
            Status = OrderStatus.Shipped;
    }

    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"The customer {field} is required.");

        return value.Trim();
    }
}

/// <summary>The product line of an order, as it was bought.</summary>
public record OrderLine(
    int ProductId,
    string Name,
    string Sku,
    string Emoji,
    decimal UnitPrice,
    int Quantity);
