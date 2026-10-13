namespace Maui.HybridApp;

/// <summary>Protocol-1 connection capabilities supplied by the host, not provider selection.</summary>
/// <param name="Commands">Explicitly registered command names.</param>
/// <param name="PreferenceNamespace">Host-chosen preference prefix; empty when no preference provider is registered.</param>
/// <param name="MaximumTextLength">Host-advertised text limit in UTF-16 code units.</param>
public sealed record ReadyResult(string[] Commands, string PreferenceNamespace, int MaximumTextLength);
