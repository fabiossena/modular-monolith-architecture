using ModularMonolith.Modules.Inventory.Domain;
using ModularMonolith.Shared.Kernel;
using Xunit;

namespace ModularMonolith.UnitTests.Domain;

public class InventoryDomainTests
{
    [Fact]
    public void Stock_item_requires_a_product()
    {
        Assert.Throws<DomainException>(() => new StockItem(0, 10, 5));
    }

    [Fact]
    public void Reserving_takes_the_units_out_of_the_stock()
    {
        var item = new StockItem(1, 10, 5);

        Assert.True(item.Reserve(3));
        Assert.Equal(7, item.Quantity);
    }

    [Fact]
    public void Reserving_more_than_available_changes_nothing()
    {
        var item = new StockItem(1, 2, 5);

        Assert.False(item.Reserve(3));
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void Reserving_a_non_positive_quantity_is_a_domain_error()
    {
        var item = new StockItem(1, 10, 5);

        Assert.Throws<DomainException>(() => item.Reserve(0));
    }

    [Fact]
    public void Adjusting_sets_an_absolute_quantity_and_never_goes_below_zero()
    {
        var item = new StockItem(1, 10, 5);

        item.AdjustTo(30, 7);
        Assert.Equal(30, item.Quantity);
        Assert.Equal(7, item.ReorderLevel);

        item.AdjustTo(-4, null);
        Assert.Equal(0, item.Quantity);
        Assert.Equal(7, item.ReorderLevel);
    }

    [Fact]
    public void Restocking_adds_units_and_never_goes_below_zero()
    {
        var item = new StockItem(1, 10, 5);

        item.AddStock(15);
        Assert.Equal(25, item.Quantity);

        item.AddStock(-100);
        Assert.Equal(0, item.Quantity);
    }
}
