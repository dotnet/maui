namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Provides access to performance tracking.
/// </summary>
internal interface IPerformanceProfiler
{
	/// <summary>
	/// Gets the layout performance tracker responsible for monitoring
	/// layout-related operations such as measure and arrange passes.
	/// </summary>
	ILayoutPerformanceTracker Layout { get; }
}