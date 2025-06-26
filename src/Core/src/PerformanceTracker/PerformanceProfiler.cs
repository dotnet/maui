#nullable disable
using System;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Implementation of <see cref="IPerformanceProfiler"/> that provides access to performance tracking.
/// </summary>
internal class PerformanceProfiler : IPerformanceProfiler
{
	public PerformanceProfiler(
		ILayoutPerformanceTracker layoutTracker)
	{
		Layout = layoutTracker;
	}
	
	/// <summary>
	/// Gets the layout performance tracker responsible for monitoring
	/// layout-related operations such as measure and arrange passes.
	/// </summary>
	/// <remarks>
	/// This tracker provides insights into UI rendering performance, helping detect 
	/// inefficiencies in layout calculations and arrangement processes.
	/// </remarks>
	public ILayoutPerformanceTracker Layout { get; }

	/// <summary>
	/// Collects and returns performance statistics related to layout operations.
	/// </summary>
	/// <returns>
	/// A <see cref="PerformanceStats"/> object that contains metrics collected
	/// from the performance trackers and a timestamp representing the moment
	/// the stats were retrieved.
	/// </returns>
	/// <remarks>
	/// This method is useful for diagnostics, telemetry, or logging purposes when evaluating
	/// rendering performance over time. The <see cref="PerformanceStats"/> returned includes
	/// details such as measure/arrange counts or durations, and when the snapshot was taken.
	/// </remarks>
	public PerformanceStats GetStats()
	{
		return new PerformanceStats
		{
			Layout = Layout.GetStats(),
			TimestampUtc = DateTime.UtcNow
		};
	}
	
	/// <summary>
	/// Records a layout measure pass with the specified duration and optional element name.
	/// </summary>
	/// <param name="duration">The duration of the measure pass in milliseconds.</param>
	/// <param name="element">Optional element name or type for per-element tracking.</param>
	/// <remarks>
	/// This method tracks layout measurements to identify performance bottlenecks 
	/// and improve UI responsiveness.
	/// </remarks>
	public void RecordMeasurePass(long duration, string element = null)
	{
		Layout.RecordMeasurePass(duration, element);
	}
	
	/// <summary>
	/// Records a layout arrange pass with the specified duration and optional element name.
	/// </summary>
	/// <param name="duration">The duration of the arrange pass in milliseconds.</param>
	/// <param name="element">Optional element name or type for per-element tracking.</param>
	/// <remarks>
	/// This method tracks layout measurements to identify performance bottlenecks 
	/// and improve UI responsiveness.
	/// </remarks>
	public void RecordArrangePass(long duration, string element = null)
	{
		Layout.RecordArrangePass(duration, element);
	}
}