using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Catalog.Contracts;

/// <summary>
/// Raised when a product is added to the catalog. The Inventory module
/// subscribes to it to open the stock record of the new product — the
/// Catalog never writes into the Inventory tables itself.
/// </summary>
public sealed record ProductCreated(
    int ProductId,
    string Sku,
    string Name,
    int InitialStock,
    int ReorderLevel) : DomainEvent;
