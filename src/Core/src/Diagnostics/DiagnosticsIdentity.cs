namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Shared name/version identity for MAUI's <see cref="System.Diagnostics.ActivitySource"/> and
/// <see cref="System.Diagnostics.Metrics.Meter"/> instances.
/// </summary>
/// <remarks>
/// Every MAUI subsystem that emits diagnostics (layout, handlers, ...) should use this identity so a
/// single <c>ActivityListener</c>/<c>MeterListener</c> subscription (matched by source name) observes
/// all of them, instead of consumers having to discover and subscribe to per-subsystem source names.
/// </remarks>
internal static class DiagnosticsIdentity
{
	public const string Namespace = "Microsoft.Maui";

	public const string Version = "1.0.0";
}
