namespace ModularMonolith.Modules.Orders.Contracts;

/// <summary>
/// The public surface of the Orders bounded context. Orders orchestrates the
/// checkout: it is the only module that coordinates Cart, Inventory and
/// Payments, and it does it inside a single transaction.
/// </summary>
public interface IOrderService
{
    Task<CheckoutResult> CheckoutAsync(
        CheckoutCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>All orders — the administration view of "what was bought".</summary>
    Task<IReadOnlyList<OrderDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>The orders of a single buyer.</summary>
    Task<IReadOnlyList<OrderDto>> ListByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<OrderDto?> GetAsync(int orderId, CancellationToken cancellationToken = default);
}
