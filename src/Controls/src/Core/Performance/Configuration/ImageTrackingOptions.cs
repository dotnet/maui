namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Provides configuration options for tracking image load performance.
/// </summary>
public class ImageTrackingOptions
{
	/// <summary>
	/// Determines whether image load time tracking is enabled.
	/// </summary>
	/// <remarks>
	/// If set to <c>true</c>, the system will monitor and log image load durations
	/// for performance profiling and optimization.
	/// </remarks>
	public bool EnableLoadTimeTracking { get; set; } = true;
}