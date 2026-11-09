using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.Modules.Catalog;
using ModularMonolith.Modules.Cart;
using ModularMonolith.Modules.Identity;
using ModularMonolith.Modules.Identity.Contracts;
using ModularMonolith.Modules.Inventory;
using ModularMonolith.Modules.Orders;
using ModularMonolith.Modules.Payments;
using ModularMonolith.Host;
using ModularMonolith.Host.Auth;
using ModularMonolith.Shared.Platform;
using ModularMonolith.Shared.Platform.Persistence;
using ModularMonolith.Shared.Platform.Security;
using ModularMonolith.Shared.Platform.Seeding;

// ---------------------------------------------------------------------------
// Modular Monolith — ONE process, ONE deployable unit, ONE relational database.
//
// The host is the composition root and nothing else: it registers the modules,
// the authentication and the entry point. Every business rule lives inside a
// module, and the modules never depend on each other except through their
// own contracts.
// ---------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Platform: the single SQLite connection, the transaction coordinator, the
// in-process event publisher and the current user.
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("Sqlite")
                       ?? "Data Source=modular-monolith.db";
builder.Services.AddModularMonolithPlatform(connectionString);

// ---------------------------------------------------------------------------
// Authentication: the host signs the tokens, the Identity module owns the
// credentials.
// ---------------------------------------------------------------------------
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicyNames.Admin, policy =>
        policy.RequireAuthenticatedUser().RequireRole(UserRoles.Admin));

    // An administrator may also buy.
    options.AddPolicy(AuthorizationPolicyNames.Buyer, policy =>
        policy.RequireAuthenticatedUser().RequireRole(UserRoles.Buyer, UserRoles.Admin));
});

// ---------------------------------------------------------------------------
// Modules: every module brings its own context, its own services and its own
// endpoints.
// ---------------------------------------------------------------------------
builder.Services.AddCatalogModule();
builder.Services.AddInventoryModule();
builder.Services.AddCartModule();
builder.Services.AddPaymentsModule();
builder.Services.AddOrdersModule();
builder.Services.AddIdentityModule();

// JSON: enums as strings (Paid / Cancelled / Approved / Declined).
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Liveness/readiness probe used by the Docker HEALTHCHECK and by compose.
builder.Services.AddHealthChecks();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Bootstrap: every module creates the tables it owns, then the seeders run.
// ---------------------------------------------------------------------------
await using (var scope = app.Services.CreateAsyncScope())
{
    var bootstrapper = scope.ServiceProvider.GetRequiredService<SchemaBootstrapper>();
    await bootstrapper.MigrateAsync();

    var seeders = scope.ServiceProvider.GetServices<IModuleSeeder>().OrderBy(seeder => seeder.Order);
    foreach (var seeder in seeders)
        await seeder.SeedAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapCatalogModule();
app.MapInventoryModule();
app.MapCartModule();
app.MapPaymentsModule();
app.MapOrdersModule();
app.MapIdentityModule();

// Self-description of the deployment (useful for the demo UI).
app.MapGet("/api/meta", () => Results.Ok(new
{
    name = "Modular Monolith Store",
    architecture = "modular monolith",
    process = "single deployable unit",
    modules = new[] { "Catalog", "Inventory", "Cart", "Payments", "Orders", "Identity" },
    database = "SQLite (single relational database, one schema per module)",
}));

app.MapHealthChecks("/health");

app.Run();
