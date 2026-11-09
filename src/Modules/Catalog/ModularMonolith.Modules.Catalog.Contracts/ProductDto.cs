namespace ModularMonolith.Modules.Catalog.Contracts;

/// <summary>A product as the rest of the system sees it.</summary>
public record ProductDto(
    int Id,
    string Sku,
    string Name,
    string Description,
    string Category,
    string Emoji,
    decimal Price,
    bool Active);

/// <summary>Command to register a new product in the catalog.</summary>
public record CreateProductCommand(
    string Sku,
    string Name,
    string Description,
    string Category,
    string Emoji,
    decimal Price,
    int InitialStock,
    int ReorderLevel);
