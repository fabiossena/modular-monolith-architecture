using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Identity.Contracts;
using ModularMonolith.Shared.Platform.Seeding;

namespace ModularMonolith.Modules.Identity.Infrastructure;

/// <summary>
/// Creates the two accounts the demo starts with: one administrator and one
/// buyer. The passwords are documented in the README.
/// </summary>
public sealed class IdentitySeeder(IdentityDbContext db, IIdentityService identity) : IModuleSeeder
{
    public int Order => 30;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
            return;

        await identity.CreateUserAsync(
            new CreateUserCommand("admin@store.com", "Admin@123", "Store Administrator", UserRoles.Admin),
            cancellationToken);

        await identity.CreateUserAsync(
            new CreateUserCommand("buyer@store.com", "Buyer@123", "Demo Buyer", UserRoles.Buyer),
            cancellationToken);
    }
}
