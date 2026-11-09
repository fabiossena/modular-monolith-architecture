using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Payments.Application.Abstractions;
using ModularMonolith.Modules.Payments.Domain;

namespace ModularMonolith.Modules.Payments.Infrastructure;

/// <summary>
/// The Payments storage. It is the only class of the module that touches the
/// payment table.
/// </summary>
public sealed class PaymentStore(PaymentsDbContext db) : IPaymentStore
{
    public void Add(Payment payment) => db.Payments.Add(payment);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public Task<Payment?> FindByOrderAsync(int orderId, CancellationToken cancellationToken = default) =>
        db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public async Task<IReadOnlyList<Payment>> FindByOrderIdsAsync(
        IReadOnlyCollection<int> orderIds,
        CancellationToken cancellationToken = default)
    {
        if (orderIds.Count == 0)
            return [];

        var ids = orderIds.Distinct().ToList();

        return await db.Payments.AsNoTracking()
            .Where(p => ids.Contains(p.OrderId))
            .ToListAsync(cancellationToken);
    }
}
