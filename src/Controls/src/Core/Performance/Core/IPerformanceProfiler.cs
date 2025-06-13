namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Provides access to performance tracking.
/// </summary>
public interface IPerformanceProfiler
{
	/// <summary>
	/// Gets the image performance tracker responsible for monitoring
	/// image loading times and related performance metrics.
	/// </summary>
	/// <remarks>
	/// This tracker helps analyze how efficiently images are loaded,
	/// enabling optimizations such as caching and format selection.
	/// </remarks>
	IImagePerformanceTracker Image { get; }
	
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
	/// Gets the performance warning manager responsible for tracking and managing potential performance issues.
	/// </summary>
	IPerformanceWarningManager Warnings { get; }
}