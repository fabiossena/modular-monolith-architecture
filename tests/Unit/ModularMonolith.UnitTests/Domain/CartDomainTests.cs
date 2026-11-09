using ModularMonolith.Modules.Cart.Domain;
using ModularMonolith.Shared.Kernel;
using Xunit;

namespace ModularMonolith.UnitTests.Domain;

public class CartDomainTests
{
    [Fact]
    public void A_cart_requires_an_id()
    {
        Assert.Throws<DomainException>(() => new Cart("  "));
    }

    [Fact]
    public void Adding_a_line_creates_it()
    {
        var cart = new Cart("cart-1");

        cart.AddLine(10, 2);

        var line = Assert.Single(cart.Items);
        Assert.Equal(10, line.ProductId);
        Assert.Equal(2, line.Quantity);
        Assert.Equal("cart-1", line.CartId);
    }

    [Fact]
    public void Adding_the_same_product_twice_increases_the_line()
    {
        var cart = new Cart("cart-1");

        cart.AddLine(10, 2);
        cart.AddLine(10, 3);

        var line = Assert.Single(cart.Items);
        Assert.Equal(5, line.Quantity);
    }

    [Fact]
    public void Setting_a_quantity_of_zero_removes_the_line()
    {
        var cart = new Cart("cart-1");
        cart.AddLine(10, 2);

        cart.SetLineQuantity(10, 0);

        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Removing_a_line_that_is_not_there_changes_nothing()
    {
        var cart = new Cart("cart-1");
        cart.AddLine(10, 2);

        cart.RemoveLine(99);

        Assert.Single(cart.Items);
    }

    [Fact]
    public void Clearing_empties_the_cart()
    {
        var cart = new Cart("cart-1");
        cart.AddLine(10, 2);
        cart.AddLine(11, 1);

        cart.Clear();

        Assert.Empty(cart.Items);
    }
}
