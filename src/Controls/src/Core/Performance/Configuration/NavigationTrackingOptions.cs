namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents the configuration options for tracking navigation performance.
/// </summary>
public class NavigationTrackingOptions
{
	/// <summary>
	/// Gets or sets a value indicating whether navigation tracking is enabled.
	/// When set to <c>true</c>, performance metrics related to navigation will be recorded.
	/// Defaults to enabled.
	/// </summary>
	public bool EnableNavigationTracking { get; set; } = true;
}