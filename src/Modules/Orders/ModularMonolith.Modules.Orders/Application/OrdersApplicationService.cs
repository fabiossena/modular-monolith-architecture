using ModularMonolith.Modules.Cart.Contracts;
using ModularMonolith.Modules.Inventory.Contracts;
using ModularMonolith.Modules.Orders.Application.Abstractions;
using ModularMonolith.Modules.Orders.Contracts;
using ModularMonolith.Modules.Orders.Domain;
using ModularMonolith.Modules.Payments.Contracts;
using ModularMonolith.Shared.Kernel;
using ModularMonolith.Shared.Platform.Persistence;
using OrderPaymentDto = ModularMonolith.Modules.Orders.Contracts.PaymentDto;

namespace ModularMonolith.Modules.Orders.Application;

/// <summary>
/// The application service of the Orders context — the orchestrator of the
/// checkout.
/// <para>
/// It is the only module that drives three other modules at once: it reads the
/// cart, reserves the stock and charges the payment. All of it happens inside
/// a single transaction that spans the Cart, Inventory, Payments and Orders
/// tables, which is exactly the guarantee a modular monolith gives over a
/// distributed one — and exactly what a set of microservices could not give
/// without a distributed transaction.
/// </para>
/// </summary>
public sealed class OrdersApplicationService(
    IOrderStore orders,
    ICartService cart,
    IInventoryService inventory,
    IPaymentService payments,
    TransactionCoordinator transactions,
    ICurrentUser currentUser) : IOrderService
{
    public async Task<CheckoutResult> CheckoutAsync(
        CheckoutCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.CartId))
            return new CheckoutResult(false, "Empty cart.", null);

        // The e-mail of the signed in buyer always wins over the one sent by
        // the browser.
        var email = currentUser.IsAuthenticated && !string.IsNullOrWhiteSpace(currentUser.Email)
            ? currentUser.Email!
            : (command.Email ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(command.CustomerName) || string.IsNullOrWhiteSpace(email))
            return new CheckoutResult(false, "Name and e-mail are required.", null);

        var cartView = await cart.GetAsync(command.CartId, cancellationToken);
        if (cartView.Items.Count == 0)
            return new CheckoutResult(false, "Empty cart.", null);

        await transactions.BeginAsync(cancellationToken);

        try
        {
            // 1) The stock is checked again inside the transaction: the
            //    availability read before the transaction may be outdated.
            foreach (var line in cartView.Items)
            {
                var available = await inventory.GetQuantityAsync(line.ProductId, cancellationToken);
                if (available < line.Quantity)
                {
                    await transactions.RollbackAsync(cancellationToken);
                    return new CheckoutResult(
                        false,
                        $"Not enough stock for \"{line.Name}\" (available: {available}).",
                        null);
                }
            }

            // 2) The order is created first: the payment needs its id.
            var order = new Order(
                command.CustomerName,
                email,
                command.Address,
                command.City,
                command.Zip,
                cartView.Items.Select(line => new OrderLine(
                    line.ProductId,
                    line.Name,
                    line.Sku,
                    line.Emoji,
                    line.Price,
                    line.Quantity)));

            orders.Add(order);
            await orders.SaveChangesAsync(cancellationToken);
            order.AssignCode(order.Id);

            // 3) The payment is charged (simulated gateway).
            var charge = await payments.ChargeAsync(
                new ChargeRequest(order.Id, cartView.Subtotal, command.PaymentMethod, command.SimulateFailure),
                cancellationToken);

            if (charge.Approved)
            {
                // 4) Approved: the stock leaves the inventory and the cart is
                //    emptied — both inside the very same transaction.
                order.MarkPaid();

                foreach (var line in cartView.Items)
                {
                    if (!await inventory.ReserveAsync(line.ProductId, line.Quantity, cancellationToken))
                    {
                        await transactions.RollbackAsync(cancellationToken);
                        return new CheckoutResult(false, $"Failed to reserve stock for \"{line.Name}\".", null);
                    }
                }

                await cart.ClearAsync(command.CartId, cancellationToken);
            }
            else
            {
                // Declined: the order is kept as cancelled and nothing leaves
                // the inventory.
                order.MarkCancelled();
            }

            await orders.SaveChangesAsync(cancellationToken);
            await transactions.CommitAsync(cancellationToken);

            return new CheckoutResult(true, charge.Reason, await GetAsync(order.Id, cancellationToken));
        }
        catch
        {
            await transactions.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<OrderDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var found = await orders.ListAsync(cancellationToken);
        return await ToDtoAsync(found, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderDto>> ListByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var found = await orders.ListByEmailAsync((email ?? string.Empty).Trim(), cancellationToken);
        return await ToDtoAsync(found, cancellationToken);
    }

    public async Task<OrderDto?> GetAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await orders.FindAsync(orderId, cancellationToken);
        if (order is null)
            return null;

        return (await ToDtoAsync([order], cancellationToken))[0];
    }

    private async Task<IReadOnlyList<OrderDto>> ToDtoAsync(
        IReadOnlyCollection<Order> found,
        CancellationToken cancellationToken)
    {
        if (found.Count == 0)
            return [];

        var charged = await payments.GetByOrderIdsAsync(found.Select(o => o.Id), cancellationToken);
        var paymentByOrder = charged.ToDictionary(p => p.OrderId);

        return found
            .Select(order =>
            {
                paymentByOrder.TryGetValue(order.Id, out var payment);

                return new OrderDto(
                    order.Id,
                    order.Code,
                    order.CustomerName,
                    order.Email,
                    order.Address,
                    order.City,
                    order.Zip,
                    order.Status,
                    order.Total,
                    order.CreatedAt,
                    order.Items
                        .Select(item => new OrderItemDto(
                            item.ProductId,
                            item.Name,
                            item.Sku,
                            item.Emoji,
                            item.UnitPrice,
                            item.Quantity))
                        .ToList(),
                    payment is null
                        ? null
                        : new OrderPaymentDto(
                            payment.Id,
                            payment.OrderId,
                            payment.Amount,
                            payment.Method,
                            payment.Status.ToString(),
                            payment.TransactionId,
                            payment.Reason,
                            payment.CreatedAt));
            })
            .ToList();
    }
}
