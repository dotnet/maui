namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>An acknowledgement of a control or write operation, not an undo guarantee.</summary>
/// <param name="Accepted">Whether the operation was accepted.</param>
public sealed record Ack(bool Accepted);
