using ModularMonolith.Modules.Payments.Domain;

namespace ModularMonolith.Modules.Payments.Application.Abstractions;

/// <summary>The port the Payments application layer uses to reach its storage.</summary>
public interface IPaymentStore
{
    void Add(Payment payment);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<Payment?> FindByOrderAsync(int orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> FindByOrderIdsAsync(
        IReadOnlyCollection<int> orderIds,
        CancellationToken cancellationToken = default);
}
