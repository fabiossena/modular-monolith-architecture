namespace ModularMonolith.Modules.Orders.Contracts;

/// <summary>Everything needed to turn a cart into an order.</summary>
public record CheckoutCommand(
    string CartId,
    string CustomerName,
    string Email,
    string Address,
    string City,
    string Zip,
    string PaymentMethod,
    bool SimulateFailure);

/// <summary>Outcome of a checkout attempt.</summary>
public record CheckoutResult(bool Ok, string? Error, OrderDto? Order);
