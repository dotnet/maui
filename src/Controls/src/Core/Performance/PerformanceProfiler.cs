#nullable disable
namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Implementation of <see cref="IPerformanceProfiler"/> that provides access to performance tracking.
/// </summary>
internal class PerformanceProfiler : IPerformanceProfiler
{
	PerformanceMonitoringOptions _options;
	
	public PerformanceProfiler(
		ILayoutPerformanceTracker layoutTracker,
		IPerformanceWarningManager warningManager)
	{
		Layout = layoutTracker;
		Warnings = warningManager;
	}
	
	/// <summary>
	/// Gets the layout performance tracker responsible for monitoring
	/// layout-related operations such as measure and arrange passes.
	/// </summary>
	public ILayoutPerformanceTracker Layout { get; }
	
	/// <summary>
	/// Gets the performance warning manager responsible for tracking and managing
	/// potential performance issues.
	/// </summary>
	public IPerformanceWarningManager Warnings { get; }
	
	/// <summary>
	/// Configures the performance monitoring options.
	/// </summary>
	/// <param name="options">The performance monitoring options to configure.</param>
	public void Configure(PerformanceMonitoringOptions options)
	{
		Layout.Configure(options.Layout);
		Warnings.Configure(options.Warnings);

		_options = options;
	}
	
	/// <summary>
	/// Gets the current performance monitoring options.
	/// </summary>
	internal PerformanceMonitoringOptions Options => _options;
	
	/// <summary>
	/// Records a layout measure pass with the specified duration and optional element name.
	/// </summary>
	/// <param name="duration">The duration of the measure pass in milliseconds.</param>
	/// <param name="element">Optional element name or type for per-element tracking.</param>
	public void RecordMeasurePass(long duration, string element = null)
	{
		Layout.RecordMeasurePass(duration, element);
	}
	
	/// <summary>
	/// Records a layout arrange pass with the specified duration and optional element name.
	/// </summary>
	/// <param name="duration">The duration of the arrange pass in milliseconds.</param>
	/// <param name="element">Optional element name or type for per-element tracking.</param>
	public void RecordArrangePass(long duration, string element = null)
	{
		Layout.RecordArrangePass(duration, element);
	}
}