namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents aggregated statistics related to navigation performance,
/// such as how long a navigation transition takes.
/// </summary>
public class NavigationStats
{
	/// <summary>
	/// Gets or sets the total duration of navigation operations.
	/// The unit is milliseconds.
	/// </summary>
	public double NavigationDuration { get; set; }
}