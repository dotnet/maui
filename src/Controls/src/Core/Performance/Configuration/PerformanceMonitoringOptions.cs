namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Aggregates all per‐domain tracking options into one central configuration object.
/// </summary>
public class PerformanceMonitoringOptions
{
	/// <summary>
	/// Gets or sets the configuration options for tracking layout performance.
	/// </summary>
	public ImageTrackingOptions Image { get; set; } = new();
	
	/// <summary>
	/// Gets or sets the image configuration options for performance monitoring.
	/// </summary>
	public LayoutTrackingOptions Layout { get; set; } = new();
	
	/// <summary>
	/// Gets or sets the warning configuration options for performance monitoring.
	/// </summary>
	public WarningOptions Warnings { get; set; } = new();
}