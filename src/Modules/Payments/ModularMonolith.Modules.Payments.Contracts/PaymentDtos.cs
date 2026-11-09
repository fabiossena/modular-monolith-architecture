namespace ModularMonolith.Modules.Payments.Contracts;

/// <summary>Outcome of a payment attempt.</summary>
public enum PaymentStatus
{
    Approved,
    Declined
}

/// <summary>A payment recorded by the Payments module.</summary>
public record PaymentDto(
    int Id,
    int OrderId,
    decimal Amount,
    string Method,
    PaymentStatus Status,
    string TransactionId,
    string? Reason,
    DateTime CreatedAt);

/// <summary>Request to charge a given amount for an order.</summary>
public record ChargeRequest(
    int OrderId,
    decimal Amount,
    string Method,
    bool SimulateFailure);

/// <summary>Result of a charge, including the payment record that was written.</summary>
public record PaymentAuthorization(
    bool Approved,
    string TransactionId,
    string? Reason,
    PaymentDto? Payment);
