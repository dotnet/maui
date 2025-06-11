using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents an update related to image loading performance.
/// </summary>
public class ImageUpdate
{
	/// <summary>
	/// The total time (in milliseconds) taken for the image to load.
	/// </summary>
	/// <remarks>
	/// This value is useful for tracking performance and identifying potential bottlenecks.
	/// </remarks>
	public double TotalTime { get; set; }

	/// <summary>
	/// The timestamp (UTC) when the image update was recorded.
	/// </summary>
	/// <remarks>
	/// Capturing this timestamp allows performance monitoring over time 
	/// and helps correlate image loading events.
	/// </remarks>
	public DateTime TimestampUtc { get; set; }
}