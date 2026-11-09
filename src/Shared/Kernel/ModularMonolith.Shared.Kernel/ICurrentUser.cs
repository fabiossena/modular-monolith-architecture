namespace ModularMonolith.Shared.Kernel;

/// <summary>
/// Read-only view of the caller that is currently executing the request.
/// Implemented by the platform layer on top of the authenticated principal and
/// injected wherever a module needs to know "who is asking".
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? Email { get; }

    string? Role { get; }

    bool IsInRole(string role);
}
