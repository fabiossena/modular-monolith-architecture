using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Shared.Platform.Seeding;

namespace ModularMonolith.Modules.Catalog.Infrastructure;

/// <summary>
/// Inserts the reference catalog on the first boot. The products are created
/// through the application service, so the Inventory module opens the stock
/// records of the new products exactly like it does for a product added by an
/// administrator.
/// </summary>
public sealed class CatalogSeeder(CatalogDbContext db, ICatalogService catalog) : IModuleSeeder
{
    public int Order => 10;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(cancellationToken))
            return;

        foreach (var product in ReferenceProducts)
        {
            await catalog.CreateAsync(
                new CreateProductCommand(
                    product.Sku,
                    product.Name,
                    product.Description,
                    product.Category,
                    product.Emoji,
                    product.Price,
                    product.Stock,
                    ReorderLevel),
                cancellationToken);
        }
    }

    private const int ReorderLevel = 8;

    private static readonly (string Sku, string Name, string Description, string Category, string Emoji, decimal Price, int Stock)[]
        ReferenceProducts =
        [
            ("KB-001", "Mechanical RGB Keyboard", "Red switches, ABNT2 (Brazilian) layout, per-key backlighting.", "Electronics", "⌨️", 349.90m, 25),
            ("MS-002", "Gaming Mouse 12000 DPI", "Optical sensor, 6 programmable buttons.", "Electronics", "🖱️", 189.90m, 40),
            ("MN-003", "Monitor 27\" IPS 144Hz", "Full HD, 1ms, premium FreeSync.", "Electronics", "🖥️", 1299.00m, 12),
            ("HF-004", "Bluetooth Headset", "Noise cancelling and detachable microphone.", "Electronics", "🎧", 499.00m, 30),
            ("CB-005", "Electric Coffee Maker 15L", "Glass carafe, permanent filter.", "Home", "☕", 129.90m, 18),
            ("VL-006", "Robot Vacuum Cleaner", "Smart mapping and mobile app.", "Home", "🤖", 899.00m, 6),
            ("TL-007", "LED Desk Lamp", "Three color temperatures, touch control.", "Home", "💡", 89.90m, 55),
            ("BK-008", "Clean Architecture (book)", "Decoupling and SOLID principles.", "Books", "📚", 79.90m, 100),
            ("BK-009", "Design Patterns GoF", "23 classic software design patterns.", "Books", "📖", 99.90m, 45),
            ("SN-010", "Lightweight Running Shoes", "Foam midsole with energy return.", "Sports", "👟", 399.00m, 22),
            ("BG-011", "Waterproof 30L Backpack", "Fits a 15\" laptop.", "Sports", "🎒", 219.90m, 14),
            ("WB-012", "Thermal Bottle 750ml", "Stainless steel, keeps drinks cold for 24h.", "Sports", "🥤", 69.90m, 60),
        ];
}
