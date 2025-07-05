namespace Microsoft.Maui.Performance;

/// <summary>
/// Provides access to performance tracking.
/// </summary>
internal interface IPerformanceProfiler
{
	/// <summary>
	/// Gets the layout performance tracker responsible for monitoring
	/// layout-related operations such as measure and arrange passes.
	/// </summary>
	/// <remarks>
	/// This tracker provides insights into UI rendering performance,
	/// helping detect inefficiencies in layout calculations and arrangement.
	/// </remarks>
	ILayoutPerformanceTracker Layout { get; }
	
	/// <summary>
	/// Retrieves the current layout performance statistics snapshot.
	/// </summary>
	/// <returns>
	/// A <see cref="PerformanceStats"/> object containing data from trackers,
	/// and a timestamp indicating when the data was captured.
	/// </returns>
	/// <remarks>
	/// This method can be used for profiling, diagnostics, or telemetry purposes
	/// to analyze how efficiently UI layout operations are executing over time.
	/// </remarks>
	PerformanceStats GetStats();
}