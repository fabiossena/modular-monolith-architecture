namespace ModularMonolith.Modules.Orders.Contracts;

/// <summary>Life cycle of an order.</summary>
public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Cancelled
}

/// <summary>A product line of an order.</summary>
public record OrderItemDto(
    int ProductId,
    string Name,
    string Sku,
    string Emoji,
    decimal UnitPrice,
    int Quantity);

/// <summary>An order as the rest of the system sees it.</summary>
public record OrderDto(
    int Id,
    string Code,
    string CustomerName,
    string Email,
    string Address,
    string City,
    string Zip,
    OrderStatus Status,
    decimal Total,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items,
    PaymentDto? Payment);
