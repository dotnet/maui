namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Aggregates all per‐domain tracking options into one central configuration object.
/// </summary>
public class PerformanceMonitoringOptions
{
	/// <summary>
	/// Options for layout‐engine tracking.
	/// </summary>
	public LayoutTrackingOptions Layout { get; set; } = new();
	
	public WarningOptions Warnings { get; set; } = new();
}