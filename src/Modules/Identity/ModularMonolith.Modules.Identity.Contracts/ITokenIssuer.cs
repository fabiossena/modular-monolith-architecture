namespace ModularMonolith.Modules.Identity.Contracts;

/// <summary>A freshly issued access token.</summary>
public record TokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    AuthenticatedUser User);

/// <summary>
/// Port implemented by the host: the Identity module authenticates the user,
/// the host turns the result into a token.
/// </summary>
public interface ITokenIssuer
{
    TokenResponse Issue(AuthenticatedUser user);
}
