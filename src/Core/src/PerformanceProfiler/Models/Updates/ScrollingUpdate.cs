using System;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Represents a single scrolling update event containing frame time, timestamp,
/// associated element, and dropped frame status.
/// </summary>
internal class ScrollingUpdate
{
	/// <summary>
	/// Gets or sets the duration of the frame in milliseconds.
	/// </summary>
	public double FrameTime { get; set; }

	/// <summary>
	/// Gets or sets the timestamp when the frame was recorded.
	/// </summary>
	public DateTime Timestamp { get; set; }

	/// <summary>
	/// Gets or sets the optional element associated with the scrolling event.
	/// </summary>
	public object? Element { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the frame was considered dropped (too slow).
	/// </summary>
	public bool IsDroppedFrame { get; set; }
}