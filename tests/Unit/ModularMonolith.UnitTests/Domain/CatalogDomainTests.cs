using ModularMonolith.Modules.Catalog.Contracts;
using ModularMonolith.Modules.Catalog.Domain;
using ModularMonolith.Shared.Kernel;
using Xunit;

namespace ModularMonolith.UnitTests.Domain;

public class CatalogDomainTests
{
    [Fact]
    public void Sku_is_normalized_to_upper_case()
    {
        Assert.Equal("KB-001", Sku.Create("  kb-001 ").Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Sku_cannot_be_empty(string value)
    {
        Assert.Throws<DomainException>(() => Sku.Create(value));
    }

    [Fact]
    public void Sku_is_a_value_object()
    {
        Assert.Equal(Sku.Create("KB-001"), Sku.Create("kb-001"));
        Assert.NotEqual(Sku.Create("KB-001"), Sku.Create("KB-002"));
    }

    [Fact]
    public void Money_rounds_to_two_decimals()
    {
        Assert.Equal(10.99m, Money.From(10.989m).Amount);
    }

    [Fact]
    public void Money_cannot_be_negative()
    {
        Assert.Throws<DomainException>(() => Money.From(-1m));
    }

    [Fact]
    public void Money_is_a_value_object()
    {
        Assert.Equal(Money.From(10m), Money.From(10m));
        Assert.NotEqual(Money.From(10m), Money.From(11m));
    }

    [Fact]
    public void Product_requires_a_name_and_a_category()
    {
        var sku = Sku.Create("KB-001");

        Assert.Throws<DomainException>(() =>
            new Product(sku, " ", "description", "Electronics", "⌨️", Money.From(10m)));

        Assert.Throws<DomainException>(() =>
            new Product(sku, "Keyboard", "description", " ", "⌨️", Money.From(10m)));
    }

    [Fact]
    public void Product_falls_back_to_a_default_emoji()
    {
        var product = new Product(Sku.Create("KB-001"), "Keyboard", "description", "Electronics", "  ", Money.From(10m));

        Assert.Equal("📦", product.Emoji);
        Assert.True(product.Active);
    }

    [Fact]
    public void Product_announces_its_creation_as_a_domain_event()
    {
        var product = new Product(Sku.Create("KB-001"), "Keyboard", "description", "Electronics", "⌨️", Money.From(349.90m));

        product.AnnounceCreated(25, 8);

        var events = product.PullDomainEvents();
        var created = Assert.IsType<ProductCreated>(Assert.Single(events));

        Assert.Equal(product.Id, created.ProductId);
        Assert.Equal("KB-001", created.Sku);
        Assert.Equal(25, created.InitialStock);
        Assert.Equal(8, created.ReorderLevel);
    }

    [Fact]
    public void Pulling_the_domain_events_empties_the_buffer()
    {
        var product = new Product(Sku.Create("KB-001"), "Keyboard", "description", "Electronics", "⌨️", Money.From(10m));
        product.AnnounceCreated(1, 1);

        Assert.Single(product.PullDomainEvents());
        Assert.Empty(product.PullDomainEvents());
    }
}
