using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents performance threshold settings for navigation operations.
/// </summary>
public class NavigationThresholds
{
	/// <summary>
	/// Gets or sets the maximum allowed time for a navigation operation.
	/// Used to determine whether a navigation action exceeds acceptable performance limits.
	/// Default is 1000 milliseconds.
	/// </summary>
	public TimeSpan NavigationMaxTime { get; set; } = TimeSpan.FromMilliseconds(1000);
}