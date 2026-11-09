using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ModularMonolith.Modules.Identity.Contracts;
using ModularMonolith.Shared.Kernel;
using ModularMonolith.Shared.Platform.Security;

namespace ModularMonolith.Modules.Identity.Api;

/// <summary>
/// The HTTP surface of the Identity module: the sign in endpoint and the
/// administration of the users.
/// </summary>
public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/login", async (
            IIdentityService identity,
            ITokenIssuer tokenIssuer,
            LoginRequest request,
            CancellationToken cancellationToken) =>
        {
            var user = await identity.AuthenticateAsync(request.Email, request.Password, cancellationToken);

            if (user is null)
                return Results.Json(
                    new { error = "Invalid e-mail or password." },
                    statusCode: StatusCodes.Status401Unauthorized);

            return Results.Ok(tokenIssuer.Issue(user));
        });

        var users = app.MapGroup("/api/users");

        users.MapGet("", async (
            IIdentityService identity,
            CancellationToken cancellationToken) =>
            Results.Ok(await identity.ListUsersAsync(cancellationToken)))
            .RequireAuthorization(AuthorizationPolicyNames.Admin);

        users.MapPost("", async (
            IIdentityService identity,
            CreateUserRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var user = await identity.CreateUserAsync(
                    new CreateUserCommand(request.Email, request.Password, request.DisplayName, request.Role),
                    cancellationToken);

                return Results.Ok(user);
            }
            catch (DomainException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }
        }).RequireAuthorization(AuthorizationPolicyNames.Admin);

        return app;
    }
}

public record LoginRequest(string Email, string Password);

public record CreateUserRequest(string Email, string Password, string DisplayName, string Role);
