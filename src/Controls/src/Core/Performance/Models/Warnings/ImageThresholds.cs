using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Defines threshold values for image loading performance monitoring.
/// </summary>
public class ImageThresholds
{
	/// <summary>
	/// Specifies the maximum allowed time for an image to load.
	/// </summary>
	/// <remarks>
	/// This value is used for performance tracking and optimization.
	/// If an image takes longer than the specified duration, it may trigger a warning or recommendation.
	/// Default value is 500 milliseconds (0.5 seconds).
	/// </remarks>
	public TimeSpan ImageMaxLoadTime { get; set; } = TimeSpan.FromMilliseconds(500);
}