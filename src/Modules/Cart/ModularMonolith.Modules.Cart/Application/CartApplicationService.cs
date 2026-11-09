using ModularMonolith.Modules.Cart.Application.Abstractions;
using ModularMonolith.Modules.Cart.Contracts;
using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Modules.Inventory.Contracts;
using CartAggregate = ModularMonolith.Modules.Cart.Domain.Cart;

namespace ModularMonolith.Modules.Cart.Application;

/// <summary>
/// The application service of the Cart context. It reads the product data
/// from the Catalog module and the availability from the Inventory module —
/// both through their public contracts, never through their tables.
/// </summary>
public sealed class CartApplicationService(
    ICartStore carts,
    ICatalogService catalog,
    IInventoryService inventory) : ICartService
{
    public async Task<CartView> GetAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var cart = await carts.ReadAsync(cartId, cancellationToken);
        return await ToViewAsync(cart, cancellationToken);
    }

    public async Task<CartView> AddAsync(
        string cartId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity < 1)
            quantity = 1;

        var product = await catalog.GetAsync(productId, cancellationToken)
                      ?? throw new KeyNotFoundException("Product not found.");

        if (!product.Active)
            throw new InvalidOperationException($"\"{product.Name}\" is not available anymore.");

        var stock = await inventory.GetQuantityAsync(productId, cancellationToken);

        var cart = await carts.FindAsync(cartId, cancellationToken);

        if (cart is null)
        {
            cart = new CartAggregate(cartId);
            carts.Add(cart);
        }

        var desired = (cart.Items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0) + quantity;

        if (desired > stock)
            throw new InvalidOperationException(stock == 0
                ? $"\"{product.Name}\" is out of stock."
                : $"Only {stock} unit(s) left of \"{product.Name}\".");

        cart.AddLine(productId, quantity);
        await carts.SaveChangesAsync(cancellationToken);

        return await GetAsync(cartId, cancellationToken);
    }

    public async Task<CartView> SetQuantityAsync(
        string cartId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var cart = await carts.FindAsync(cartId, cancellationToken);
        if (cart is null)
            return await GetAsync(cartId, cancellationToken);

        var line = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (line is null)
            return await GetAsync(cartId, cancellationToken);

        if (quantity <= 0)
        {
            cart.RemoveLine(productId);
            carts.RemoveLines([line]);
        }
        else
        {
            var stock = await inventory.GetQuantityAsync(productId, cancellationToken);
            if (quantity > stock)
                throw new InvalidOperationException($"Only {stock} unit(s) left in stock.");

            cart.SetLineQuantity(productId, quantity);
        }

        await carts.SaveChangesAsync(cancellationToken);
        return await GetAsync(cartId, cancellationToken);
    }

    public async Task<CartView> RemoveAsync(
        string cartId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        var cart = await carts.FindAsync(cartId, cancellationToken);
        if (cart is null)
            return await GetAsync(cartId, cancellationToken);

        var line = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (line is not null)
        {
            cart.RemoveLine(productId);
            carts.RemoveLines([line]);
            await carts.SaveChangesAsync(cancellationToken);
        }

        return await GetAsync(cartId, cancellationToken);
    }

    public async Task<CartView> ClearAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var cart = await carts.FindAsync(cartId, cancellationToken);
        if (cart is null)
            return await GetAsync(cartId, cancellationToken);

        carts.RemoveLines(cart.Items.ToList());
        cart.Clear();
        await carts.SaveChangesAsync(cancellationToken);

        return await GetAsync(cartId, cancellationToken);
    }

    private async Task<CartView> ToViewAsync(CartAggregate? cart, CancellationToken cancellationToken)
    {
        if (cart is null || cart.Items.Count == 0)
            return new CartView(cart?.Id ?? string.Empty, [], 0m, 0);

        var productIds = cart.Items.Select(i => i.ProductId).ToList();

        var products = await catalog.GetManyAsync(productIds, cancellationToken);
        var byId = products.ToDictionary(p => p.Id);
        var stocks = await inventory.GetQuantitiesAsync(productIds, cancellationToken);

        var lines = cart.Items
            .Where(line => byId.ContainsKey(line.ProductId))
            .Select(line =>
            {
                var product = byId[line.ProductId];
                stocks.TryGetValue(line.ProductId, out var stock);

                return new CartLineView(
                    line.ProductId,
                    product.Name,
                    product.Emoji,
                    product.Sku,
                    product.Price,
                    line.Quantity,
                    stock,
                    product.Price * line.Quantity);
            })
            .ToList();

        return new CartView(
            cart.Id,
            lines,
            lines.Sum(l => l.Subtotal),
            lines.Sum(l => l.Quantity));
    }
}
