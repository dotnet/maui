namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>A safe command error on the wire.</summary>
/// <param name="Code">The stable error identifier.</param>
/// <param name="Message">A safe user-facing explanation.</param>
public sealed record CommandError(string Code, string Message);
