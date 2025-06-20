using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Defines threshold limits for layout performance tracking.
/// </summary>
public class LayoutThresholds
{
	/// <summary>
	/// Maximum allowed duration in milliseconds for a layout measure pass before raising a warning.
	/// </summary>
	public TimeSpan LayoutMaxMeasureTime { get; set; } = TimeSpan.FromMilliseconds(25);
	
	/// <summary>
	/// Maximum allowed duration in milliseconds for a layout arrange pass before raising a warning.
	/// </summary>
	public TimeSpan LayoutMaxArrangeTime { get; set; } = TimeSpan.FromMilliseconds(15);
}