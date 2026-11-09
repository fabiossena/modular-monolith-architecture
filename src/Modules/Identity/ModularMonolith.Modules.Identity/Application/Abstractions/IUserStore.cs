using ModularMonolith.Modules.Identity.Domain;

namespace ModularMonolith.Modules.Identity.Application.Abstractions;

/// <summary>The port the Identity application layer uses to reach its storage.</summary>
public interface IUserStore
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);

    void Add(User user);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
