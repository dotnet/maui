namespace DotNet.Release;

/// <summary>
/// Represents an expected, actionable release validation failure that the CLI reports
/// without an exception stack trace.
/// </summary>
internal sealed class DotNetReleaseException : Exception
{
    public DotNetReleaseException(string message)
        : base(message)
    {
    }

    public DotNetReleaseException(IEnumerable<string> messages)
        : this(string.Join(Environment.NewLine, messages))
    {
    }
}
