using ModularMonolith.Modules.Orders.Domain;

namespace ModularMonolith.Modules.Orders.Application.Abstractions;

/// <summary>The port the Orders application layer uses to reach its storage.</summary>
public interface IOrderStore
{
    Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> ListByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Order?> FindAsync(int orderId, CancellationToken cancellationToken = default);

    void Add(Order order);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
