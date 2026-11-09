using ModularMonolith.Modules.Payments.Contracts;
using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Payments.Domain;

/// <summary>
/// The Payment aggregate: the record of a payment attempt made for an order.
/// It belongs to the Payments module — the Orders module only keeps a
/// reference to it.
/// </summary>
public sealed class Payment : AggregateRoot<int>
{
    public int OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Method { get; private set; } = "card";
    public PaymentStatus Status { get; private set; }
    public string TransactionId { get; private set; } = string.Empty;
    public string? Reason { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Payment()
    {
        // Required by EF Core.
    }

    private Payment(int orderId, decimal amount, string method, PaymentStatus status, string transactionId, string? reason)
    {
        if (orderId <= 0)
            throw new DomainException("The order id is required.");

        if (amount < 0m)
            throw new DomainException("The amount cannot be negative.");

        OrderId = orderId;
        Amount = amount;
        Method = string.IsNullOrWhiteSpace(method) ? "card" : method.Trim();
        Status = status;
        TransactionId = transactionId;
        Reason = reason;
        CreatedAt = DateTime.UtcNow;
    }

    public static Payment Approved(int orderId, decimal amount, string method, string transactionId) =>
        new(orderId, amount, method, PaymentStatus.Approved, transactionId, null);

    public static Payment Declined(int orderId, decimal amount, string method, string reason) =>
        new(orderId, amount, method, PaymentStatus.Declined, string.Empty, reason);
}
