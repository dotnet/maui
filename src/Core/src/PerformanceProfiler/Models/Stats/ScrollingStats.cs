namespace Microsoft.Maui.Performance;

/// <summary>
/// Provides scrolling performance statistics such as average frame time, dropped frames, total scroll duration, and frame count.
/// </summary>
internal class ScrollingStats
{
	/// <summary>
	/// Gets or sets the average time per frame in milliseconds.
	/// </summary>
	public double AverageFrameTime { get; set; }

	/// <summary>
	/// Gets or sets the number of frames that exceeded the target frame time.
	/// </summary>
	public int DroppedFrames { get; set; }

	/// <summary>
	/// Gets or sets the total duration of scrolling in milliseconds.
	/// </summary>
	public double TotalScrollDuration { get; set; }

	/// <summary>
	/// Gets or sets the total number of frames recorded.
	/// </summary>
	public int FrameCount { get; set; }
}