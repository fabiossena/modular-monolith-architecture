using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Kernel;
using Xunit;

namespace ModularMonolith.ArchitectureTests;

/// <summary>
/// The host is the composition root: it wires the modules and owns the entry
/// point. It must not contain a single business rule.
/// </summary>
public class HostCompositionTests
{
    [Fact]
    public void The_host_declares_no_aggregate()
    {
        var aggregates = Modules.Host
            .GetTypes()
            .Where(type => type.IsClass)
            .Where(type => IsOrInheritsFrom(type, typeof(AggregateRoot<>)))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(aggregates.Count == 0, $"The host declares aggregates: {string.Join(", ", aggregates)}.");
    }

    [Fact]
    public void The_host_declares_no_entity()
    {
        var entities = Modules.Host
            .GetTypes()
            .Where(type => type.IsClass)
            .Where(type => IsOrInheritsFrom(type, typeof(Entity<>)))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(entities.Count == 0, $"The host declares entities: {string.Join(", ", entities)}.");
    }

    [Fact]
    public void The_host_declares_no_database_context()
    {
        var contexts = Modules.Host
            .GetTypes()
            .Where(type => typeof(DbContext).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(contexts.Count == 0, $"The host declares database contexts: {string.Join(", ", contexts)}.");
    }

    [Fact]
    public void The_host_has_no_domain_namespace()
    {
        var namespaces = Modules.Host
            .GetTypes()
            .Select(type => type.Namespace ?? string.Empty)
            .Where(ns => ns.Contains("Domain", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        Assert.True(namespaces.Count == 0, $"The host has domain namespaces: {string.Join(", ", namespaces)}.");
    }

    [Fact]
    public void The_host_only_holds_the_entry_point_and_the_token_issuer()
    {
        var publicTypes = Modules.Host
            .GetTypes()
            .Where(type => type.IsPublic)
            .Select(type => type.FullName!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        Assert.All(publicTypes, name =>
            Assert.True(
                name.StartsWith("ModularMonolith.Host", StringComparison.Ordinal),
                $"The host declares the type '{name}'."));
    }

    private static bool IsOrInheritsFrom(Type type, Type genericDefinition)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericDefinition)
                return true;
        }

        return false;
    }
}
