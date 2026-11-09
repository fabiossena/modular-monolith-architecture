using System.Reflection;

namespace ModularMonolith.ArchitectureTests;

/// <summary>
/// The inventory of the modules: the name of the bounded context and the
/// assemblies that belong to it.
/// </summary>
public sealed record ModuleDefinition(
    string Name,
    Assembly Implementation,
    Assembly Contracts)
{
    public string NamespacePrefix => $"ModularMonolith.Modules.{Name}";
}

public static class Modules
{
    public static IReadOnlyList<ModuleDefinition> All { get; } =
    [
        new("Catalog",
            typeof(ModularMonolith.Modules.Catalog.Infrastructure.CatalogDbContext).Assembly,
            typeof(ModularMonolith.Modules.Catalog.Contracts.ICatalogService).Assembly),
        new("Inventory",
            typeof(ModularMonolith.Modules.Inventory.Infrastructure.InventoryDbContext).Assembly,
            typeof(ModularMonolith.Modules.Inventory.Contracts.IInventoryService).Assembly),
        new("Cart",
            typeof(ModularMonolith.Modules.Cart.Infrastructure.CartDbContext).Assembly,
            typeof(ModularMonolith.Modules.Cart.Contracts.ICartService).Assembly),
        new("Payments",
            typeof(ModularMonolith.Modules.Payments.Infrastructure.PaymentsDbContext).Assembly,
            typeof(ModularMonolith.Modules.Payments.Contracts.IPaymentService).Assembly),
        new("Orders",
            typeof(ModularMonolith.Modules.Orders.Infrastructure.OrdersDbContext).Assembly,
            typeof(ModularMonolith.Modules.Orders.Contracts.IOrderService).Assembly),
        new("Identity",
            typeof(ModularMonolith.Modules.Identity.Infrastructure.IdentityDbContext).Assembly,
            typeof(ModularMonolith.Modules.Identity.Contracts.IIdentityService).Assembly),
    ];

    public static Assembly Kernel { get; } =
        typeof(ModularMonolith.Shared.Kernel.AggregateRoot<>).Assembly;

    public static Assembly Platform { get; } =
        typeof(ModularMonolith.Shared.Platform.Persistence.ModuleDbContext).Assembly;

    public static Assembly Host { get; } =
        typeof(ModularMonolith.Host.JwtOptions).Assembly;

    /// <summary>The assemblies of the modules, implementation and contracts.</summary>
    public static IEnumerable<Assembly> ModuleAssemblies =>
        All.SelectMany(module => new[] { module.Implementation, module.Contracts });
}
