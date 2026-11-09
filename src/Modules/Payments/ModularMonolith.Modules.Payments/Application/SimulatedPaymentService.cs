using ModularMonolith.Modules.Payments.Application.Abstractions;
using ModularMonolith.Modules.Payments.Contracts;
using ModularMonolith.Modules.Payments.Domain;

namespace ModularMonolith.Modules.Payments.Application;

/// <summary>
/// The application service of the Payments context. The gateway is simulated
/// so the demo stays self-contained, but the contract is the one a real
/// provider would implement: a charge is approved or declined, never both.
/// </summary>
public sealed class SimulatedPaymentService(IPaymentStore payments) : IPaymentService
{
    public IReadOnlyList<string> SupportedMethods { get; } = ["card", "pix", "boleto"];

    public async Task<PaymentAuthorization> ChargeAsync(
        ChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        // Deterministic simulation rules:
        //  - the caller asks for a failure -> declined
        //  - a non positive amount         -> declined
        //  - otherwise                      -> approved
        var declined = request.SimulateFailure || request.Amount <= 0m;

        var payment = declined
            ? Payment.Declined(
                request.OrderId,
                request.Amount,
                request.Method,
                "Transaction declined by the issuer (simulated).")
            : Payment.Approved(
                request.OrderId,
                request.Amount,
                request.Method,
                $"TXN-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}");

        payments.Add(payment);
        await payments.SaveChangesAsync(cancellationToken);

        return new PaymentAuthorization(!declined, payment.TransactionId, payment.Reason, ToDto(payment));
    }

    public async Task<PaymentDto?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var payment = await payments.FindByOrderAsync(orderId, cancellationToken);
        return payment is null ? null : ToDto(payment);
    }

    public async Task<IReadOnlyList<PaymentDto>> GetByOrderIdsAsync(
        IEnumerable<int> orderIds,
        CancellationToken cancellationToken = default)
    {
        var found = await payments.FindByOrderIdsAsync(orderIds.Distinct().ToList(), cancellationToken);
        return found.Select(ToDto).ToList();
    }

    private static PaymentDto ToDto(Payment payment) =>
        new(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Method,
            payment.Status,
            payment.TransactionId,
            payment.Reason,
            payment.CreatedAt);
}
