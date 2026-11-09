using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Modules.Inventory.Domain;
using ModularMonolith.Shared.Kernel;

namespace ModularMonolith.Modules.Inventory.Infrastructure;

/// <summary>
/// Reacts to a product that was added to the catalog by opening its stock
/// record. The Catalog module never writes into the Inventory tables: it only
/// announces what happened through a domain event.
/// </summary>
public sealed class ProductCreatedHandler(InventoryDbContext db) : IDomainEventHandler<ProductCreated>
{
    public async Task HandleAsync(ProductCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var existing = await db.StockItems
            .FirstOrDefaultAsync(i => i.ProductId == domainEvent.ProductId, cancellationToken);

        if (existing is not null)
            return;

        db.StockItems.Add(new StockItem(
            domainEvent.ProductId,
            domainEvent.InitialStock,
            domainEvent.ReorderLevel));

        await db.SaveChangesAsync(cancellationToken);
    }
}
