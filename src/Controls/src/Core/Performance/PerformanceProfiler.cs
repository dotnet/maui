namespace Microsoft.Maui.Controls.Performance;

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
	/// Gets the layout performance tracker.
	/// </summary>
	public ILayoutPerformanceTracker Layout { get; }
	
	/// <summary>
	/// Configures the performance monitoring options.
	/// </summary>
	/// <param name="options">The performance monitoring options to configure.</param>

	public void Configure(PerformanceMonitoringOptions options)
	{
		Layout.Configure(options.Layout);
	}
	
	/// <summary>
	/// Records a layout measure pass with the specified duration and optional element name.
	/// </summary>
	/// <param name="duration">The duration of the measure pass in milliseconds.</param>
	/// <param name="element">Optional element name or type for per-element tracking.</param>
	public void RecordMeasurePass(long duration, string? element = null)
	{
		Layout.RecordMeasurePass(duration, element);
	}
}