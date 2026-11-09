using ModularMonolith.Modules.Identity.Application;
using ModularMonolith.Modules.Identity.Contracts;
using ModularMonolith.Modules.Identity.Domain;
using ModularMonolith.Shared.Kernel;
using Xunit;

namespace ModularMonolith.UnitTests.Domain;

public class IdentityDomainTests
{
    [Fact]
    public void A_user_requires_an_email_and_a_display_name()
    {
        Assert.Throws<DomainException>(() => new User(" ", "Jane", "hash", UserRoles.Buyer));
        Assert.Throws<DomainException>(() => new User("jane@email.com", " ", "hash", UserRoles.Buyer));
    }

    [Fact]
    public void The_email_is_normalized_to_lower_case()
    {
        var user = new User(" Jane@Email.COM ", "Jane", "hash", UserRoles.Buyer);

        Assert.Equal("jane@email.com", user.Email);
        Assert.Equal("Jane", user.DisplayName);
        Assert.True(user.Active);
    }

    [Fact]
    public void An_unknown_role_is_refused()
    {
        Assert.Throws<DomainException>(() => new User("jane@email.com", "Jane", "hash", "Wizard"));
    }

    [Fact]
    public void The_role_is_normalized_to_the_known_spelling()
    {
        var user = new User("jane@email.com", "Jane", "hash", "admin");

        Assert.Equal(UserRoles.Admin, user.Role);
    }

    [Fact]
    public void A_password_can_be_verified()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Buyer@123");

        Assert.True(hasher.Verify("Buyer@123", hash));
        Assert.False(hasher.Verify("wrong", hash));
    }

    [Fact]
    public void Two_hashes_of_the_same_password_are_different()
    {
        var hasher = new PasswordHasher();

        var first = hasher.Hash("Buyer@123");
        var second = hasher.Hash("Buyer@123");

        Assert.NotEqual(first, second);
        Assert.True(hasher.Verify("Buyer@123", first));
        Assert.True(hasher.Verify("Buyer@123", second));
    }

    [Fact]
    public void A_broken_hash_is_never_valid()
    {
        var hasher = new PasswordHasher();

        Assert.False(hasher.Verify("Buyer@123", "not-a-hash"));
        Assert.False(hasher.Verify("Buyer@123", string.Empty));
    }
}
