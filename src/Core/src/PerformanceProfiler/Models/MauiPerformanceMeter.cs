using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Performance;

/// <summary>
/// A wrapper class for the MAUI performance Meter instance.
/// </summary>
/// <remarks>
/// This wrapper prevents DI container conflicts by providing a unique type for MAUI's performance meter,
/// rather than registering Meter directly which could conflict with other libraries or application code
/// that also register Meter instances.
/// </remarks>
internal class MauiPerformanceMeter
{
	/// <summary>
	/// Gets the underlying Meter instance used for MAUI performance monitoring.
	/// </summary>
	public Meter Meter { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MauiPerformanceMeter"/> class.
	/// </summary>
	public MauiPerformanceMeter()
	{
		Meter = new Meter("Microsoft.Maui");
	}
}