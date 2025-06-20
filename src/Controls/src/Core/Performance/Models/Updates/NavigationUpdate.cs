using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents a single navigation performance data point,
/// capturing how long a navigation action took and when it occurred.
/// </summary>
public class NavigationUpdate
{
	/// <summary>
	/// Gets or sets the duration (in milliseconds)
	/// that a navigation operation took to complete.
	/// </summary>
	public double NavigationDuration { get; set; }

	/// <summary>
	/// Gets or sets the timestamp of when the navigation occurred.
	/// Useful for chronological analysis or filtering.
	/// </summary>
	public DateTime Timestamp { get; set; }
}