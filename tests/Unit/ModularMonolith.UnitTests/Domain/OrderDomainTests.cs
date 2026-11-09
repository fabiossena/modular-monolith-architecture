using ModularMonolith.Modules.Orders.Contracts;
using ModularMonolith.Modules.Orders.Domain;
using ModularMonolith.Shared.Kernel;
using Xunit;

namespace ModularMonolith.UnitTests.Domain;

public class OrderDomainTests
{
    private static Order NewOrder() =>
        new(
            "Jane Doe",
            "jane@email.com",
            "123 Flower Street",
            "Portland",
            "97201",
            [
                new OrderLine(1, "Keyboard", "KB-001", "⌨️", 349.90m, 2),
                new OrderLine(2, "Mouse", "MS-002", "🖱️", 189.90m, 1),
            ]);

    [Fact]
    public void An_order_requires_a_name_and_an_email()
    {
        Assert.Throws<DomainException>(() => new Order(" ", "jane@email.com", "", "", "", []));
        Assert.Throws<DomainException>(() => new Order("Jane", " ", "", "", "", []));
    }

    [Fact]
    public void The_total_is_the_sum_of_the_lines()
    {
        var order = NewOrder();

        Assert.Equal(2, order.Items.Count);
        Assert.Equal(349.90m * 2 + 189.90m, order.Total);
    }

    [Fact]
    public void A_new_order_is_pending()
    {
        Assert.Equal(OrderStatus.Pending, NewOrder().Status);
    }

    [Fact]
    public void The_code_is_built_from_the_identifier()
    {
        var order = NewOrder();

        order.AssignCode(42);

        Assert.Equal("ORD-00042", order.Code);
    }

    [Fact]
    public void A_paid_order_can_be_shipped_but_a_shipped_order_cannot_be_cancelled()
    {
        var order = NewOrder();

        order.MarkPaid();
        Assert.Equal(OrderStatus.Paid, order.Status);

        order.MarkShipped();
        Assert.Equal(OrderStatus.Shipped, order.Status);

        order.MarkCancelled();
        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public void A_paid_order_can_be_cancelled()
    {
        var order = NewOrder();

        order.MarkPaid();
        order.MarkCancelled();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void A_cancelled_order_cannot_be_paid()
    {
        var order = NewOrder();

        order.MarkCancelled();
        order.MarkPaid();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }
}
