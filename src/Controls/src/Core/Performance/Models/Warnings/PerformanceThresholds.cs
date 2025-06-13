namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents performance thresholds for different aspects of performance.
/// </summary>
public class PerformanceThresholds
{
	/// <summary>
	/// Gets or sets the image performance thresholds, defining acceptable limits for image loading times.
	/// </summary>
	/// <remarks>
	/// This property helps monitor and optimize image rendering performance to ensure smooth user experiences.
	/// </remarks>
	public ImageThresholds Image { get; set; } = new();
	
	/// <summary>
	/// Gets or sets the layout performance thresholds, which define acceptable limits for various layout metrics.
	/// </summary>
	/// <remarks>
	/// These thresholds help track layout rendering efficiency and identify potential bottlenecks affecting UI responsiveness.
	/// </remarks>
	public LayoutThresholds Layout { get; set; } = new();
}