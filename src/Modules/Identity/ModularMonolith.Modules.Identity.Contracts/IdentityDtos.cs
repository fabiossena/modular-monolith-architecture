namespace ModularMonolith.Modules.Identity.Contracts;

/// <summary>The roles known by the Identity module.</summary>
public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Buyer = "Buyer";

    public static readonly IReadOnlyList<string> All = [Admin, Buyer];
}

/// <summary>The authenticated user, without any credential material.</summary>
public record AuthenticatedUser(
    string Id,
    string Email,
    string DisplayName,
    string Role);

/// <summary>A user of the system.</summary>
public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    string Role,
    bool Active,
    DateTime CreatedAt);

/// <summary>Command to create a user.</summary>
public record CreateUserCommand(
    string Email,
    string Password,
    string DisplayName,
    string Role);
