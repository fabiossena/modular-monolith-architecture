using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Identity.Application.Abstractions;
using ModularMonolith.Modules.Identity.Domain;

namespace ModularMonolith.Modules.Identity.Infrastructure;

/// <summary>
/// The Identity storage. It is the only class of the module that touches the
/// user table.
/// </summary>
public sealed class UserStore(IdentityDbContext db) : IUserStore
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.Users.AsNoTracking()
            .OrderBy(u => u.DisplayName)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking().AnyAsync(u => u.Email == email, cancellationToken);

    public void Add(User user) => db.Users.Add(user);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
