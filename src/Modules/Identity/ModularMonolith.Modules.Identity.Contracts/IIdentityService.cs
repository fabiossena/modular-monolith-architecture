namespace ModularMonolith.Modules.Identity.Contracts;

/// <summary>
/// The public surface of the Identity bounded context. The module knows how to
/// verify a credential; it does not know how a token is built — that is the
/// job of the host, behind the <see cref="ITokenIssuer"/> port.
/// </summary>
public interface IIdentityService
{
    Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<UserDto> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken cancellationToken = default);

    Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
