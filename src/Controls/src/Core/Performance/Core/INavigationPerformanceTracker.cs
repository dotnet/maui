using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Provides functionality to track and monitor navigation performance within an application.
/// </summary>
public interface INavigationPerformanceTracker
{
	/// <summary>
	/// Configures the performance tracker with specific navigation tracking options.
	/// </summary>
	/// <param name="options">Settings that define how navigation tracking should behave.</param>
	void Configure(NavigationTrackingOptions options);

	/// <summary>
	/// Retrieves aggregated statistics related to navigation performance.
	/// </summary>
	/// <returns>An instance of <see cref="NavigationStats"/> representing collected navigation metrics.</returns>
	NavigationStats GetStats();

	/// <summary>
	/// Subscribes to real-time navigation updates, allowing consumers to receive performance data as it becomes available.
	/// </summary>
	/// <param name="callback">An action to invoke with each new <see cref="NavigationUpdate"/> instance.</param>
	void SubscribeToNavigationUpdates(Action<NavigationUpdate> callback);

	/// <summary>
	/// Records a navigation event by capturing its duration and contextual metadata,
	/// and emits metrics, updates subscribers, and evaluates performance thresholds.
	/// </summary>
	/// <param name="duration">The total time taken for the navigation operation, in milliseconds.</param>
	void RecordNavigation(double duration);
}