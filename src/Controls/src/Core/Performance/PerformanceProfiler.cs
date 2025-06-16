#nullable disable
namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Implementation of <see cref="IPerformanceProfiler"/> that provides access to performance tracking.
/// </summary>
internal class PerformanceProfiler : IPerformanceProfiler
{
	PerformanceMonitoringOptions _options;
	
	public PerformanceProfiler(
		IImagePerformanceTracker imageTracker,
		ILayoutPerformanceTracker layoutTracker,
		INavigationPerformanceTracker navigationTracker,
		IPerformanceWarningManager warningManager)
	{
		Image = imageTracker;
		Layout = layoutTracker;
		Navigation = navigationTracker;
		Warnings = warningManager;
	}
	
	/// <summary>
	/// Gets the image performance tracker responsible for monitoring
	/// image loading times and related performance metrics.
	/// </summary>
	/// <remarks>
	/// This tracker helps analyze image rendering efficiency, allowing optimizations 
	/// such as caching strategies and format selection to improve performance.
	/// </remarks>
	public IImagePerformanceTracker Image { get; }
	
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
	/// Gets the navigation performance tracker responsible for analyzing
	/// navigation timings.
	/// </summary>
	/// <remarks>
	/// This tracker helps identify performance issues during page transitions, such as slow pushes, pops, or modal navigations,
	/// and supports metrics collection and threshold-based warnings.
	/// </remarks>
	public INavigationPerformanceTracker Navigation { get; }
	
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
		Image.Configure(options.Image);
		Layout.Configure(options.Layout);
		Navigation.Configure(options.Navigation);
		Warnings.Configure(options.Warnings);

		_options = options;
	}
	
	/// <summary>
	/// Gets the current performance monitoring options.
	/// </summary>
	internal PerformanceMonitoringOptions Options => _options;
	
	/// <summary>
	/// Records the duration of an image load event for performance tracking.
	/// </summary>
	/// <param name="loadDuration">The measured time (in milliseconds) taken to load the image.</param>
	/// <remarks>
	/// This method captures image loading times to help analyze rendering efficiency 
	/// and optimize performance where necessary.
	/// </remarks>
	public void RecordImageLoad(double loadDuration)
	{
		Image.RecordImageLoad(loadDuration);
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
	
	/// <summary>
	/// Records a navigation operation with the specified duration for performance tracking.
	/// </summary>
	/// <param name="duration">The time taken to complete a navigation action, in milliseconds.</param>
	/// <remarks>
	/// This method logs navigation durations to help identify latency in page navigations
	/// and improve the perceived responsiveness of navigation flows.
	/// </remarks>
	public void RecordNavigation(long duration)
	{
		Navigation.RecordNavigation(duration);
	}
}