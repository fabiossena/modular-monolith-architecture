using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Modules.Inventory.Domain;
using ModularMonolith.Modules.Inventory.Contracts;
using ModularMonolith.Shared.Platform.Seeding;

namespace ModularMonolith.Modules.Inventory.Infrastructure;

/// <summary>
/// Makes sure every product of the catalog has a stock record. The products
/// of the reference catalog already get one through the <c>ProductCreated</c>
/// event; this seeder only repairs a database that was interrupted in the
/// middle of the creation.
/// </summary>
public sealed class InventorySeeder(InventoryDbContext db, ICatalogService catalog) : IModuleSeeder
{
    public int Order => 20;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var products = await catalog.SearchAsync(null, null, cancellationToken);
        var known = await db.StockItems.AsNoTracking()
            .Select(i => i.ProductId)
            .ToListAsync(cancellationToken);

        foreach (var product in products.Where(p => !known.Contains(p.Id)))
        {
            db.StockItems.Add(new StockItem(product.Id, 0, DefaultReorderLevel));
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private const int DefaultReorderLevel = 8;
}
