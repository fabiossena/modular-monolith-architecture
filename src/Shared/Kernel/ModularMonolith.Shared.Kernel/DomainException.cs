namespace ModularMonolith.Shared.Kernel;

/// <summary>
/// Raised when a domain invariant is violated. It carries no infrastructure
/// dependency so every module can catch it in its own application layer.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
