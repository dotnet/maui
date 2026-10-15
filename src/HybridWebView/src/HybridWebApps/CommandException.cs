namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>A deliberate, safe-to-display command failure, distinct from unexpected native exceptions.</summary>
/// <param name="code">The stable wire error code.</param>
/// <param name="message">A safe explanation without secrets or raw native exception details.</param>
public sealed class CommandException(string code, string message) : Exception(message)
{
	/// <summary>Gets the stable wire error code.</summary>
	public string Code { get; } = code;
}
