namespace ModularMonolith.Modules.Payments.Contracts;

/// <summary>
/// The public surface of the Payments bounded context. The gateway is
/// simulated, but the contract is the one a real provider would implement.
/// </summary>
public interface IPaymentService
{
    Task<PaymentAuthorization> ChargeAsync(
        ChargeRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentDto?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentDto>> GetByOrderIdsAsync(
        IEnumerable<int> orderIds,
        CancellationToken cancellationToken = default);

    /// <summary>The payment methods accepted by the gateway.</summary>
    IReadOnlyList<string> SupportedMethods { get; }
}
