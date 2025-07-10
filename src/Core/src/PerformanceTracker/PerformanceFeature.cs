using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Controls whether performance monitoring features are enabled.
/// This class is used by the trimmer to remove performance monitoring code in production builds.
/// </summary>
internal class PerformanceFeature
{
	/// <summary>
	/// Indicates whether performance monitoring is enabled.
	/// </summary>
#if NET9_0_OR_GREATER
	[FeatureSwitchDefinition("System.Diagnostics.Metrics.Meter.IsSupported")]
#endif
	internal static bool IsMetricsSupported { get; }
		= InitializeIsMeterSupported();
	
	static bool InitializeIsMeterSupported() =>
		!AppContext.TryGetSwitch(
			"System.Diagnostics.Metrics.Meter.IsSupported",
			out bool isSupported) || isSupported;
}