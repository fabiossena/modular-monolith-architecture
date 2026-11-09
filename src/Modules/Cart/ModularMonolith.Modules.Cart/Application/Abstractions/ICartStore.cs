using CartAggregate = ModularMonolith.Modules.Cart.Domain.Cart;

namespace ModularMonolith.Modules.Cart.Application.Abstractions;

/// <summary>
/// The port the Cart application layer uses to reach its own storage.
/// </summary>
public interface ICartStore
{
    /// <summary>Reads a cart without tracking it (used to build the view).</summary>
    Task<CartAggregate?> ReadAsync(string cartId, CancellationToken cancellationToken = default);

    /// <summary>Reads a cart for modification (tracked).</summary>
    Task<CartAggregate?> FindAsync(string cartId, CancellationToken cancellationToken = default);

    void Add(CartAggregate cart);

    void RemoveLines(IEnumerable<Domain.CartLine> lines);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
