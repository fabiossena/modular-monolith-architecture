namespace ModularMonolith.Modules.Orders.Contracts;

/// <summary>The payment attached to an order.</summary>
public record PaymentDto(
    int Id,
    int OrderId,
    decimal Amount,
    string Method,
    string Status,
    string TransactionId,
    string? Reason,
    DateTime CreatedAt);
