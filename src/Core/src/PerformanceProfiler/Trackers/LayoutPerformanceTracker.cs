using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Performance
{
	internal class LayoutPerformanceTracker : ILayoutPerformanceTracker
	{
		readonly Histogram<double> _measureDurationHistogram;
		readonly Histogram<double> _arrangeDurationHistogram;
		
		double _measureDuration;
		double _arrangeDuration;
	
		readonly List<LayoutUpdate> _updateHistory = new List<LayoutUpdate>();
		readonly object _historyLock = new object();
		
		readonly List<Action<LayoutUpdate>> _subscribers = new List<Action<LayoutUpdate>>();
		readonly object _subscriberLock = new object();
		
		public LayoutPerformanceTracker(Meter meter)
		{
			_measureDurationHistogram = meter.CreateHistogram<double>(
				"maui.layout.measure.duration",
				unit: "ms",
				description: "Duration of layout measure passes");

			_arrangeDurationHistogram = meter.CreateHistogram<double>(
				"maui.layout.arrange.duration",
				unit: "ms",
				description: "Duration of layout arrange passes");
		}
		
		/// <summary>
		/// Gets the layout performance statistics.
		/// </summary>
		/// <returns>A <see cref="LayoutStats"/> object containing measure and arrange pass data.</returns>
		public LayoutStats GetStats()
		{
			return new LayoutStats
			{
				MeasureDuration = _measureDuration,
				ArrangeDuration = _arrangeDuration,
			};
		}
		
		/// <summary>
		/// Gets the history of layout updates, optionally filtered by element.
		/// </summary>
		/// <param name="element">Optional element to filter by instance.</param>
		/// <returns>A list of <see cref="LayoutUpdate"/> records matching the filter criteria.</returns>
		/// <exception cref="ArgumentException">Thrown if both element and elementType are provided.</exception>
		public IEnumerable<LayoutUpdate> GetHistory(object? element = null)
		{
			lock (_historyLock)
			{
				if (element is null)
					return new List<LayoutUpdate>(_updateHistory);

				var filtered = new List<LayoutUpdate>();
				
				foreach (var update in _updateHistory)
				{
					if (Equals(update.Element, element))
					{
						filtered.Add(update);
					}
				}

				return filtered;
			}
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
		public void RecordMeasurePass(double duration, object? element = null)
		{
			// Update fields
			_measureDuration = duration;

			// Record metrics
			var tags = new TagList();
			if (element is not null)
			{
				tags.Add("element.type", element.GetType().Name);
			}

			_measureDurationHistogram.Record(duration, tags);
			
			var update = new LayoutUpdate(
				LayoutPassType.Measure,
				duration,
				element ?? string.Empty,
				DateTime.UtcNow);

			lock (_historyLock)
			{
				_updateHistory.Add(update);
			}
			
			PublishLayoutUpdate(update);
		}

		/// <summary>
		/// Record an Arrange pass of the layout engine.
		/// </summary>
		public void RecordArrangePass(double duration, object? element = null)
		{
			// Update fields
			_arrangeDuration = duration;

			// Record metrics
			var tags = new TagList();
			if (element is not null)
			{
				tags.Add("element.type", element.GetType().Name);
			}

			_arrangeDurationHistogram.Record(duration, tags);
			
			// Publish LayoutUpdate
			var update = new LayoutUpdate(
				LayoutPassType.Arrange,
				duration,
				element ?? string.Empty,
				DateTime.UtcNow);

			lock (_historyLock)
			{
				_updateHistory.Add(update);
			}
			
			PublishLayoutUpdate(update);
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
	}
}