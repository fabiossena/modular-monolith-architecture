using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Orders.Application.Abstractions;
using ModularMonolith.Modules.Orders.Domain;

namespace ModularMonolith.Modules.Orders.Infrastructure;

/// <summary>
/// The Orders storage. It is the only class of the module that touches the
/// order tables.
/// </summary>
public sealed class OrderStore(OrdersDbContext db) : IOrderStore
{
    public async Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByEmailAsync(
        string email,
        CancellationToken cancellationToken = default) =>
        await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.Email == email)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Order?> FindAsync(int orderId, CancellationToken cancellationToken = default) =>
        db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

    public void Add(Order order) => db.Orders.Add(order);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
