using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Controls.Performance
{
	internal class LayoutPerformanceTracker : ILayoutPerformanceTracker
	{
		readonly Counter<long> _measurePassCount;
		readonly Histogram<double> _measureDuration;
		readonly Counter<long> _arrangePassCount;
		readonly Histogram<double> _arrangeDuration;

		readonly IPerformanceWarningManager _warningManager;
		
		// Aggregation fields
		long _totalMeasurePasses;
		double _totalMeasureDuration;
		
		long _totalArrangePasses;
		double _totalArrangeDuration;
		
		// List of subscribers for layout‐update callbacks
		readonly List<Action<LayoutUpdate>> _subscribers = new List<Action<LayoutUpdate>>();
		readonly object _subscriberLock = new object();

		LayoutTrackingOptions _options = new();

		public LayoutPerformanceTracker(Meter meter, IPerformanceWarningManager? warningManager = null)
		{
			_measurePassCount = meter.CreateCounter<long>(
				"maui.layout.measure.count",
				unit: "ops",
				description: "Number of layout measure passes");
			_measureDuration = meter.CreateHistogram<double>(
				"maui.layout.measure.duration",
				unit: "ms",
				description: "Duration of layout measure passes");

			_arrangePassCount = meter.CreateCounter<long>(
				"maui.layout.arrange.count",
				unit: "ops",
				description: "Number of layout arrange passes");
			_arrangeDuration = meter.CreateHistogram<double>(
				"maui.layout.arrange.duration",
				unit: "ms",
				description: "Duration of layout arrange passes");
			
			_warningManager = warningManager ?? new PerformanceWarningManager();
		}

		/// <summary>
		/// Configures the layout tracking options for this tracker.
		/// </summary>
		/// <param name="options">Options to control layout tracking behavior.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
		public void Configure(LayoutTrackingOptions options)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
		}

		/// <summary>
		/// Gets the configured layout tracking options.
		/// </summary>
		public LayoutTrackingOptions Options => _options;
		
		/// <summary>
		/// Gets the layout performance statistics.
		/// </summary>
		/// <returns>A <see cref="LayoutStats"/> object containing measure and arrange pass data.</returns>
		public LayoutStats GetStats()
		{
			return new LayoutStats
			{
				MeasurePassCount = _totalMeasurePasses,
				MeasureDuration = _totalMeasureDuration,
				ArrangePassCount = _totalArrangePasses,
				ArrangeDuration = _totalArrangeDuration,
			};
		}

		/// <summary>
		/// Subscribe to receive real‐time LayoutUpdate events (Measure or Arrange).
		/// </summary>
		public void SubscribeToLayoutUpdates(Action<LayoutUpdate> callback)
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
		/// Record a Measure pass of the layout engine.
		/// </summary>
		public void RecordMeasurePass(double duration, string? element = null)
		{
			if (!_options.EnableMeasurePassTracking)
			{
				return;
			}

			// Update aggregation fields
			_totalMeasurePasses++;
			_totalMeasureDuration = duration;

			// Record metrics
			var tags = new TagList();
			if (_options.TrackPerElement && !string.IsNullOrEmpty(element))
			{
				tags.Add("element.type", element);
			}

			_measurePassCount.Add(1, tags);
			_measureDuration.Record(duration, tags);
			
			var update = new LayoutUpdate(
				LayoutPassType.Measure,
				duration,
				element ?? string.Empty,
				DateTime.UtcNow);

			PublishLayoutUpdate(update);
			
			// Check layout thresholds and generate warnings
			CheckLayoutThresholds(duration, element);
		}

		/// <summary>
		/// Record an Arrange pass of the layout engine.
		/// </summary>
		public void RecordArrangePass(double duration, string? element = null)
		{
			if (!_options.EnableArrangePassTracking)
			{
				return;
			}

			// Update aggregation fields
			_totalArrangePasses++;
			_totalArrangeDuration = duration;

			// Record metrics
			var tags = new TagList();
			if (_options.TrackPerElement && !string.IsNullOrEmpty(element))
			{
				tags.Add("element.type", element);
			}

			_arrangePassCount.Add(1, tags);
			_arrangeDuration.Record(duration, tags);
			
			// Publish LayoutUpdate
			var update = new LayoutUpdate(
				LayoutPassType.Arrange,
				duration,
				element ?? string.Empty,
				DateTime.UtcNow);

			PublishLayoutUpdate(update);
			
			// Check layout thresholds and generate warnings
			CheckLayoutThresholds(duration, element);
		}

		/// <summary>
		/// Internally notify all subscribers about a new layout update.
		/// </summary>
		void PublishLayoutUpdate(LayoutUpdate update)
		{
			Action<LayoutUpdate>[] subscribersSnapshot;
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
		/// Check measure pass performance and generate warnings if thresholds are exceeded.
		/// </summary>
		void CheckLayoutThresholds(double duration, string? element)
		{
			const string category = "Layout";

			var warningOptions = _warningManager.Options;
			LayoutThresholds layoutThresholds = warningOptions.Thresholds.Layout;
			
			_warningManager.CheckThreshold(
				category,
				"ArrangePassDuration",
				duration,
				layoutThresholds.LayoutMaxArrangeTime.TotalMilliseconds,
				$"Arrange pass for {element ?? "unknown element"} took {duration}ms. Consider simplifying layout or using more efficient containers.");
			
			_warningManager.CheckThreshold(
				category,
				"MeasurePassDuration",
				duration,
				layoutThresholds.LayoutMaxMeasureTime.TotalMilliseconds,
				$"Measure pass for {element ?? "unknown element"} took {duration}ms. Consider simplifying layout or using more efficient containers.");
		}
	}
}