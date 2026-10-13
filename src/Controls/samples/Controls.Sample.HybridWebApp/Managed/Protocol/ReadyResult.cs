namespace Maui.HybridApp;

/// <summary>Protocol-1 connection capabilities supplied by the host, not provider selection.</summary>
/// <param name="Commands">Explicitly registered command names.</param>
/// <param name="PreferenceNamespace">Host-chosen preference prefix; empty when no preference provider is registered.</param>
/// <param name="MaximumTextLength">Host-advertised text limit in UTF-16 code units.</param>
/// <param name="InteractiveCommands">Commands registered with an interactive OS UI policy; null for older hosts.</param>
/// <param name="InteractiveTimeoutMilliseconds">Bounded native interactive budget; the client allows three additional transport seconds.</param>
/// <param name="InitialLocation">Validated bundle-relative entry URL; frontend routing is not native product policy.</param>
public sealed record ReadyResult(string[] Commands, string PreferenceNamespace, int MaximumTextLength,
	string[]? InteractiveCommands = null, int InteractiveTimeoutMilliseconds = 300000, string? InitialLocation = null);
