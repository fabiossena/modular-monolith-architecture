using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Catalog.Application.Abstractions;
using ModularMonolith.Modules.Catalog.Domain;

namespace ModularMonolith.Modules.Catalog.Infrastructure;

/// <summary>
/// The Catalog storage. It is the only class of the module that touches the
/// Catalog tables.
/// </summary>
public sealed class ProductStore(CatalogDbContext db) : IProductStore
{
    public async Task<IReadOnlyList<Product>> SearchAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking().Where(p => p.Active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term) ||
                p.Sku.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        return await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public Task<Product?> FindAsync(int productId, CancellationToken cancellationToken = default) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

    public async Task<IReadOnlyList<Product>> FindByIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return [];

        var ids = productIds.Distinct().ToList();

        return await db.Products.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> CategoriesAsync(CancellationToken cancellationToken = default) =>
        await db.Products.AsNoTracking()
            .Where(p => p.Active)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default) =>
        db.Products.AsNoTracking().AnyAsync(p => p.Sku == sku, cancellationToken);

    public void Add(Product product) => db.Products.Add(product);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
