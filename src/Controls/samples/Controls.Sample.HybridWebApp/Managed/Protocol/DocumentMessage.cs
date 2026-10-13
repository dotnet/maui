namespace Maui.HybridApp;

/// <summary>A bounded raw document lease notice.</summary>
/// <param name="Session">The document UUID.</param>
/// <param name="Kind">Heartbeat or disconnect.</param>
public sealed record DocumentMessage(string Session, string Kind);
