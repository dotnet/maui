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
	internal static bool IsSupported { get; }
		= InitializeIsMeterSupported();
	
	/// <summary>
	/// Guards execution of performance monitoring code.
	/// When IsEnabled is false, the linker will remove the guarded code entirely.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool Guard()
	{
		return IsSupported;
	}
	
	static bool InitializeIsMeterSupported() =>
		!AppContext.TryGetSwitch(
			"System.Diagnostics.Metrics.Meter.IsSupported",
			out bool isSupported) || isSupported;
}