using System.Reflection;

namespace ModularMonolith.ArchitectureTests;

/// <summary>
/// Reads the types a type really depends on: the base types, the interfaces,
/// the fields, the properties, the signatures of the methods and the
/// attributes. It is used by the rules that must not be fooled by a
/// namespace matching.
/// </summary>
public static class DependencyScanner
{
    public static IReadOnlySet<string> NamespacesOf(Type type)
    {
        var namespaces = new HashSet<string>(StringComparer.Ordinal);

        Add(type, namespaces);

        foreach (var @interface in type.GetInterfaces())
            Add(@interface, namespaces);

        foreach (var attribute in type.GetCustomAttributesData())
        {
            Add(attribute.AttributeType, namespaces);

            foreach (var argument in attribute.ConstructorArguments)
                Add(argument.ArgumentType, namespaces);
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            Add(field.FieldType, namespaces);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            Add(property.PropertyType, namespaces);

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            Add(method.ReturnType, namespaces);

            foreach (var parameter in method.GetParameters())
                Add(parameter.ParameterType, namespaces);
        }

        foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            foreach (var parameter in constructor.GetParameters())
                Add(parameter.ParameterType, namespaces);
        }

        return namespaces;
    }

    private static void Add(Type type, ISet<string> namespaces)
    {
        if (type is null)
            return;

        if (type.Namespace is { Length: > 0 })
            namespaces.Add(type.Namespace);

        foreach (var argument in type.GetGenericArguments())
            Add(argument, namespaces);
    }
}
