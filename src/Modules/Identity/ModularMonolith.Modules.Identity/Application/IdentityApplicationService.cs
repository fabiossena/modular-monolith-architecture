using ModularMonolith.Modules.Identity.Application.Abstractions;
using ModularMonolith.Modules.Identity.Contracts;
using ModularMonolith.Modules.Identity.Domain;
using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Identity.Application;

/// <summary>
/// The application service of the Identity context: it verifies the
/// credentials and manages the users. It knows nothing about tokens — the
/// host turns an <see cref="AuthenticatedUser"/> into one.
/// </summary>
public sealed class IdentityApplicationService(
    IUserStore users,
    PasswordHasher passwordHasher) : IIdentityService
{
    public async Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            return null;

        var normalized = email.Trim().ToLowerInvariant();

        var user = await users.FindByEmailAsync(normalized, cancellationToken);
        if (user is null || !user.Active)
            return null;

        return passwordHasher.Verify(password, user.PasswordHash)
            ? ToAuthenticatedUser(user)
            : null;
    }

    public async Task<UserDto> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalized = (command.Email ?? string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new DomainException("The e-mail is required.");

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
            throw new DomainException("The password must have at least 6 characters.");

        if (await users.ExistsAsync(normalized, cancellationToken))
            throw new DomainException($"There is already a user with the e-mail '{normalized}'.");

        var user = new User(
            normalized,
            command.DisplayName,
            passwordHasher.Hash(command.Password),
            command.Role);

        users.Add(user);
        await users.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var found = await users.ListAsync(cancellationToken);
        return found.Select(ToDto).ToList();
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await users.FindByEmailAsync(normalized, cancellationToken);

        return user is null ? null : ToDto(user);
    }

    private static AuthenticatedUser ToAuthenticatedUser(User user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role);

    private static UserDto ToDto(User user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role, user.Active, user.CreatedAt);
}
