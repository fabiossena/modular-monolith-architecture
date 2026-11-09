namespace ModularMonolith.Host;

/// <summary>Settings of the JSON Web Token issued by the host.</summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "ModularMonolith";

    public string Audience { get; set; } = "ModularMonolith.Client";

    public string Key { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 120;
}
