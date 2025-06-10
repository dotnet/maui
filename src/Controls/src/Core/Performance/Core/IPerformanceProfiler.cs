namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Provides access to performance tracking.
/// </summary>
public interface IPerformanceProfiler
{
	/// <summary>
	/// Gets the layout performance tracker responsible for monitoring
	/// layout-related operations such as measure and arrange passes.
	/// </summary>
	ILayoutPerformanceTracker Layout { get; }
	
	/// <summary>
	/// Gets the performance warning manager responsible for tracking and managing potential performance issues.
	/// </summary>
	IPerformanceWarningManager Warnings { get; }
}