using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Shared.Platform.Security;

/// <summary>
/// Reads the caller from the current HTTP request. It is the platform
/// implementation of the <see cref="ICurrentUser"/> contract of the kernel:
/// modules ask "who is asking?" without knowing anything about ASP.NET Core.
/// </summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? UserId => Find(ClaimTypes.NameIdentifier) ?? Find("sub");

    public string? Email => Find(ClaimTypes.Email) ?? Find("email");

    public string? Role => Find(ClaimTypes.Role) ?? Find("role");

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    private string? Find(string claimType) => Principal?.FindFirst(claimType)?.Value;
}
