using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace ModularMonolith.ArchitectureTests;

/// <summary>
/// The rules of the layers inside a module: the domain knows nothing about
/// the storage, the application layer talks to the storage only through its
/// own ports and the API layer only through the application layer.
/// </summary>
public class ModuleLayeringTests
{
    public static IEnumerable<object[]> ModulesData =>
        Modules.All.Select(module => new object[] { module.Name, module.Implementation });

    [Theory]
    [MemberData(nameof(ModulesData))]
    public void The_domain_does_not_depend_on_the_storage(string name, Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceMatching($@"^ModularMonolith\.Modules\.{name}\.Domain(\.|$)")
            .Should()
            .NotHaveDependencyOnAny($"ModularMonolith.Modules.{name}.Infrastructure")
            .GetResult();

        ArchitectureAssert.RuleIsSatisfied(result, $"The domain of {name} depends on its own infrastructure");
    }

    [Theory]
    [MemberData(nameof(ModulesData))]
    public void The_domain_does_not_depend_on_the_api(string name, Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceMatching($@"^ModularMonolith\.Modules\.{name}\.Domain(\.|$)")
            .Should()
            .NotHaveDependencyOnAny($"ModularMonolith.Modules.{name}.Api")
            .GetResult();

        ArchitectureAssert.RuleIsSatisfied(result, $"The domain of {name} depends on its own api");
    }

    [Theory]
    [MemberData(nameof(ModulesData))]
    public void The_application_layer_does_not_depend_on_the_storage(string name, Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceMatching($@"^ModularMonolith\.Modules\.{name}\.Application(\.|$)")
            .Should()
            .NotHaveDependencyOnAny($"ModularMonolith.Modules.{name}.Infrastructure")
            .GetResult();

        ArchitectureAssert.RuleIsSatisfied(result, $"The application layer of {name} depends on its own infrastructure");
    }

    [Theory]
    [MemberData(nameof(ModulesData))]
    public void The_api_layer_does_not_reach_the_storage(string name, Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceMatching($@"^ModularMonolith\.Modules\.{name}\.Api(\.|$)")
            .Should()
            .NotHaveDependencyOnAny($"ModularMonolith.Modules.{name}.Infrastructure")
            .GetResult();

        ArchitectureAssert.RuleIsSatisfied(result, $"The api layer of {name} reaches its own infrastructure");
    }

    [Theory]
    [MemberData(nameof(ModulesData))]
    public void The_domain_does_not_depend_on_entity_framework(string name, Assembly assembly)
    {
        var domainTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceMatching($@"^ModularMonolith\.Modules\.{name}\.Domain(\.|$)")
            .GetTypes();

        foreach (var type in domainTypes)
        {
            var forbidden = DependencyScanner.NamespacesOf(type)
                .Where(@namespace => @namespace.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
                .ToList();

            Assert.True(
                forbidden.Count == 0,
                $"The domain type '{type.FullName}' depends on {string.Join(", ", forbidden)}.");
        }
    }

    [Theory]
    [MemberData(nameof(ModulesData))]
    public void The_domain_does_not_depend_on_asp_net(string name, Assembly assembly)
    {
        var domainTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceMatching($@"^ModularMonolith\.Modules\.{name}\.Domain(\.|$)")
            .GetTypes();

        foreach (var type in domainTypes)
        {
            var forbidden = DependencyScanner.NamespacesOf(type)
                .Where(@namespace => @namespace.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal))
                .ToList();

            Assert.True(
                forbidden.Count == 0,
                $"The domain type '{type.FullName}' depends on {string.Join(", ", forbidden)}.");
        }
    }
}
