namespace DotNet.Release;

internal sealed class DotNetReleaseException : Exception
{
    public DotNetReleaseException(string message) : base(message)
    {
    }

    public DotNetReleaseException(IEnumerable<string> messages) : this(string.Join(Environment.NewLine, messages))
    {
    }
}
