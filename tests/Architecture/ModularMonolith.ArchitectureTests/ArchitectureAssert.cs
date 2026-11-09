using NetArchTest.Rules;
using Xunit;

namespace ModularMonolith.ArchitectureTests;

/// <summary>
/// Helpers to report the result of a NetArchTest rule in a readable way.
/// </summary>
internal static class ArchitectureAssert
{
    /// <summary>
    /// Asserts that a rule was satisfied and names the types that broke it.
    /// </summary>
    public static void RuleIsSatisfied(TestResult result, string what)
    {
        var offenders = result.FailingTypeNames is { Count: > 0 }
            ? string.Join(", ", result.FailingTypeNames)
            : "unknown types";

        Assert.True(result.IsSuccessful, $"{what}: {offenders}.");
    }
}
