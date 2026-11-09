using System.Reflection;
using Xunit;

namespace ModularMonolith.ArchitectureTests;

/// <summary>
/// The rules that make the modules really independent. They are checked on
/// the compiled assemblies, not on the folder structure: a violation is a
/// compile error or a broken dependency, never a convention.
/// </summary>
public class ModuleBoundaryTests
{
    [Fact]
    public void A_module_only_references_the_contracts_of_the_other_modules()
    {
        foreach (var module in Modules.All)
        {
            var forbidden = module.Implementation
                .GetReferencedAssemblies()
                .Select(reference => reference.Name!)
                .Where(name => name.StartsWith("ModularMonolith.Modules.", StringComparison.Ordinal))
                .Where(name => !name.EndsWith(".Contracts", StringComparison.Ordinal))
                .ToList();

            Assert.True(
                forbidden.Count == 0,
                $"The module {module.Name} references the implementation of another module: {string.Join(", ", forbidden)}.");
        }
    }

    [Fact]
    public void A_contracts_assembly_never_references_a_module_implementation()
    {
        foreach (var module in Modules.All)
        {
            var forbidden = module.Contracts
                .GetReferencedAssemblies()
                .Select(reference => reference.Name!)
                .Where(name => name.StartsWith("ModularMonolith.Modules.", StringComparison.Ordinal))
                .Where(name => !name.EndsWith(".Contracts", StringComparison.Ordinal))
                .ToList();

            Assert.True(
                forbidden.Count == 0,
                $"The contracts of {module.Name} reference an implementation: {string.Join(", ", forbidden)}.");
        }
    }

    [Fact]
    public void A_module_never_references_its_own_contracts_from_another_module()
    {
        foreach (var module in Modules.All)
        {
            var referenced = module.Implementation
                .GetReferencedAssemblies()
                .Select(reference => reference.Name!)
                .ToList();

            Assert.Contains(module.Contracts.GetName().Name, referenced);
        }
    }

    [Fact]
    public void The_shared_parts_never_reference_a_module()
    {
        foreach (var shared in new[] { Modules.Kernel, Modules.Platform })
        {
            var forbidden = shared
                .GetReferencedAssemblies()
                .Select(reference => reference.Name!)
                .Where(name => name.StartsWith("ModularMonolith.Modules.", StringComparison.Ordinal))
                .ToList();

            Assert.True(
                forbidden.Count == 0,
                $"{shared.GetName().Name} references a module: {string.Join(", ", forbidden)}.");
        }
    }

    [Fact]
    public void The_kernel_is_free_of_any_framework()
    {
        var forbidden = Modules.Kernel
            .GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .Where(name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
                           || name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)
                           || name.StartsWith("ModularMonolith.", StringComparison.Ordinal))
            .ToList();

        Assert.True(forbidden.Count == 0, $"The kernel references {string.Join(", ", forbidden)}.");
    }

    [Fact]
    public void The_host_only_knows_the_modules_and_the_shared_parts()
    {
        var forbidden = Modules.Host
            .GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .Where(name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
            .ToList();

        Assert.True(
            forbidden.Count == 0,
            $"The host touches the database directly: {string.Join(", ", forbidden)}.");
    }

    [Fact]
    public void Every_module_has_exactly_one_contracts_assembly()
    {
        var contracts = Modules.All
            .Select(module => module.Contracts.GetName().Name!)
            .ToList();

        Assert.Equal(contracts.Count, contracts.Distinct(StringComparer.Ordinal).Count());
        Assert.All(contracts, name => Assert.EndsWith(".Contracts", name, StringComparison.Ordinal));
    }
}
