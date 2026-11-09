using ModularMonolith.Modules.Identity.Contracts;
using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Identity.Domain;

/// <summary>
/// The User aggregate: who may sign in and what they may do. The module owns
/// the credentials — the password hash never leaves the Identity module.
/// </summary>
public sealed class User : AggregateRoot<string>
{
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = UserRoles.Buyer;
    public bool Active { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private User()
    {
        // Required by EF Core.
    }

    public User(string email, string displayName, string passwordHash, string role)
    {
        Id = Guid.NewGuid().ToString("N");
        Email = Normalize(email);
        DisplayName = Required(displayName, "display name");
        PasswordHash = Required(passwordHash, "password hash");
        Role = NormalizeRole(role);
        Active = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => Active = false;

    private static string Normalize(string? email) =>
        Required(email, "e-mail").ToLowerInvariant();

    private static string NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return UserRoles.Buyer;

        var value = role.Trim();

        if (!UserRoles.All.Contains(value, StringComparer.OrdinalIgnoreCase))
            throw new DomainException($"Unknown role '{value}'.");

        return UserRoles.All.First(r => r.Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"The {field} is required.");

        return value.Trim();
    }
}
