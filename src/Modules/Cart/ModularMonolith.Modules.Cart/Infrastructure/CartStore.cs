using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Cart.Application.Abstractions;
using CartAggregate = ModularMonolith.Modules.Cart.Domain.Cart;

namespace ModularMonolith.Modules.Cart.Infrastructure;

/// <summary>
/// The Cart storage. It is the only class of the module that touches the
/// cart tables.
/// </summary>
public sealed class CartStore(CartDbContext db) : ICartStore
{
    public Task<CartAggregate?> ReadAsync(string cartId, CancellationToken cancellationToken = default) =>
        db.Carts.AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

    public Task<CartAggregate?> FindAsync(string cartId, CancellationToken cancellationToken = default) =>
        db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

    public void Add(CartAggregate cart) => db.Carts.Add(cart);

    public void RemoveLines(IEnumerable<Domain.CartLine> lines) => db.CartLines.RemoveRange(lines);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
