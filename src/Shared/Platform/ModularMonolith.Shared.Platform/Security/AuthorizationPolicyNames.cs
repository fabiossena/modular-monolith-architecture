namespace ModularMonolith.Shared.Platform.Security;

/// <summary>
/// Names of the authorization policies the host configures. Modules refer to
/// the policies by name, so no module has to depend on another one to know
/// which role guards an endpoint.
/// </summary>
public static class AuthorizationPolicyNames
{
    /// <summary>Reserved for the administrators of the store.</summary>
    public const string Admin = "Admin";

    /// <summary>Reserved for the buyers of the store.</summary>
    public const string Buyer = "Buyer";
}
