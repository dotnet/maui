using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Controls.Performance;

public class NavigationPerformanceTracker : INavigationPerformanceTracker
{
	readonly Histogram<double> _navigationDurationHistogram;
	
	readonly IPerformanceWarningManager _warningManager;

	double _navigationDuration;
	
	NavigationTrackingOptions _options = new();
	
	// List of subscribers for navigation‐update callbacks
	readonly List<Action<NavigationUpdate>> _subscribers = new List<Action<NavigationUpdate>>();
	readonly object _subscriberLock = new object();
	
	public NavigationPerformanceTracker(Meter meter, IPerformanceWarningManager? warningManager = null)
	{
		_navigationDurationHistogram = meter.CreateHistogram<double>(
			"maui.navigation.duration", 
			unit: "ms", 
			description: "Page navigation duration in milliseconds");
		
		_warningManager = warningManager ?? new PerformanceWarningManager();
	}

	/// <summary>
	/// Applies the specified navigation tracking options to configure behavior.
	/// </summary>
	/// <param name="options">The tracking options to apply.</param>
	/// <exception cref="ArgumentNullException">Thrown if the options argument is null.</exception>

	public void Configure(NavigationTrackingOptions options)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
	}
	
	/// <summary>
	/// Gets the current tracking configuration for navigation metrics.
	/// </summary>
	public NavigationTrackingOptions Options => _options;
	
	/// <summary>
	/// Retrieves a snapshot of the current aggregated navigation statistics.
	/// </summary>
	/// <returns>A <see cref="NavigationStats"/> object containing summary metrics.</returns>
	public NavigationStats GetStats()
	{
		return new NavigationStats
		{
			NavigationDuration = _navigationDuration
		};
	}

	/// <summary>
	/// Registers a callback to receive updates whenever new navigation performance data is available.
	/// </summary>
	/// <param name="callback">The method to invoke when navigation data is recorded.</param>
	/// <exception cref="ArgumentNullException">Thrown if the callback is null.</exception>
	public void SubscribeToNavigationUpdates(Action<NavigationUpdate> callback)
	{
		if (callback == null)
		{
			throw new ArgumentNullException(nameof(callback));
		}

		lock (_subscriberLock)
		{
			_subscribers.Add(callback);
		}
	}

	/// <summary>
	/// Records a navigation event by capturing its duration and contextual metadata,
	/// and emits metrics, updates subscribers, and evaluates performance thresholds.
	/// </summary>
	/// <param name="duration">The total time taken for the navigation operation, in milliseconds.</param>
	public void RecordNavigation(double duration)
	{
		if (!_options.EnableNavigationTracking)
		{  
			return;
		}
		
		// Update fields
		_navigationDuration = duration;
		
		// Record metrics
		_navigationDurationHistogram.Record(duration);

		var update = new NavigationUpdate
		{
			NavigationDuration = duration,
		};

		PublishNavigationUpdate(update);
		
		// Check navigation thresholds and generate warnings
		CheckNavigationThresholds(duration);
	}
	
	/// <summary>
	/// Internally notify all subscribers about a new navigation update.
	/// </summary>
	void PublishNavigationUpdate(NavigationUpdate update)
	{
		Action<NavigationUpdate>[] subscribersSnapshot;
		lock (_subscriberLock)
		{
			subscribersSnapshot = _subscribers.ToArray();
		}

		foreach (var subscriber in subscribersSnapshot)
		{
			try
			{
				subscriber.Invoke(update);
			}
			catch
			{
				// Swallow exceptions from subscribers to avoid breaking the tracker.
			}
		}
	}
	
	/// <summary>
	/// Evaluates the navigation duration against configured thresholds and issues a performance warning if exceeded.
	/// </summary>
	/// <param name="duration">The time taken to complete a navigation operation, in milliseconds.</param>
	void CheckNavigationThresholds(double duration)
	{
		const string category = "Navigation";

		var warningOptions = _warningManager.Options;
		NavigationThresholds navigationThresholds = warningOptions.Thresholds.Navigation;

		_warningManager.CheckThreshold(
			category,
			"NavigationDuration",
			duration,
			navigationThresholds.NavigationMaxTime.TotalMilliseconds,
			$"Navigation took {duration}ms, exceeding the expected maximum of {navigationThresholds.NavigationMaxTime.TotalMilliseconds}ms. Consider optimizing page construction, reducing resource loading, or delaying non-critical UI updates.");
	}
}