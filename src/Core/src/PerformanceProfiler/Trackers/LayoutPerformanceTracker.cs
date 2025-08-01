using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Performance
{
    /// <summary>
    /// Tracks performance metrics for layout operations, including measure and arrange passes.
    /// </summary>
    internal class LayoutPerformanceTracker : ILayoutPerformanceTracker
    {
        readonly Histogram<double> _measureDurationHistogram;
        readonly Histogram<double> _arrangeDurationHistogram;
        double _measureDuration;
        double _arrangeDuration;
        readonly List<LayoutUpdate> _updateHistory = new();
        readonly object _historyLock = new();
        readonly List<WeakReference<Action<LayoutUpdate>>> _subscribers = new();
        readonly object _subscriberLock = new();
        
        // Tracks cumulative duration spent on direct children for each element and pass type
        readonly ConditionalWeakTable<object, ConcurrentDictionary<LayoutPassType, double>> _childDurations = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutPerformanceTracker"/> class with the specified meter for metrics collection.
        /// </summary>
        /// <param name="mauiPerformanceMeter">The meter used to create histograms for tracking measure and arrange durations.</param>
        public LayoutPerformanceTracker(MauiPerformanceMeter mauiPerformanceMeter)
        {
            _measureDurationHistogram = mauiPerformanceMeter.Meter.CreateHistogram<double>(
                "maui.layout.measure.duration",
                unit: "ms",
                description: "Duration of layout measure passes");

            _arrangeDurationHistogram = mauiPerformanceMeter.Meter.CreateHistogram<double>(
                "maui.layout.arrange.duration",
                unit: "ms",
                description: "Duration of layout arrange passes");
        }

        /// <summary>
        /// Retrieves the latest performance statistics for layout operations.
        /// </summary>
        /// <returns>A <see cref="LayoutStats"/> object containing durations for measure and arrange passes.</returns>
        public LayoutStats GetStats()
        {
            return new LayoutStats
            {
                MeasureDuration = _measureDuration,
                ArrangeDuration = _arrangeDuration
            };
        }

        /// <summary>
        /// Retrieves the history of layout updates, optionally filtered by element.
        /// </summary>
        /// <param name="element">The element to filter updates by, or null to return all updates.</param>
        /// <returns>An enumerable of <see cref="LayoutUpdate"/> objects representing the layout history.</returns>
        public IEnumerable<LayoutUpdate> GetHistory(object? element = null)
        {
	        lock (_historyLock)
	        {
		        if (element is null)
		        {
			        return new List<LayoutUpdate>(_updateHistory);
		        }

		        var filtered = new List<LayoutUpdate>();
		        foreach (var update in _updateHistory)
		        {
			        if (update.ElementRef?.TryGetTarget(out var target) == true && 
			            ReferenceEquals(target, element))
			        {
				        filtered.Add(update);
			        }
		        }
		        return filtered;
	        }
        }

        /// <summary>
        /// Subscribes to real-time layout update events for measure and arrange passes.
        /// </summary>
        /// <param name="callback">The callback to invoke when a layout update occurs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is null.</exception>
        public void SubscribeToLayoutUpdates(Action<LayoutUpdate> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            lock (_subscriberLock)
            {
                _subscribers.Add(new WeakReference<Action<LayoutUpdate>>(callback));
            }
        }

        /// <summary>
        /// Records a measure pass for the specified element, adjusting durations for nested children if applicable.
        /// </summary>
        /// <param name="duration">The duration of the measure pass in milliseconds.</param>
        /// <param name="element">The element being measured, or null if not specified.</param>
        public void RecordMeasurePass(double duration, object? element = null)
        {
            double standaloneDuration = duration;

            if (element is not null)
            {
                bool hasChildren = element is ILayout layout && layout.Count > 0;
                object? parent = element is IView view ? view.Parent : null;

                if (parent is not null)
                {
                    var parentDurations = _childDurations.GetOrCreateValue(parent);
                    parentDurations.AddOrUpdate(
                        LayoutPassType.Measure,
                        duration,
                        (key, oldValue) => oldValue + duration);
                }

                if (element is ILayout)
                {
                    if (_childDurations.TryGetValue(element, out var elementDurations) &&
                        elementDurations.TryGetValue(LayoutPassType.Measure, out var childTime))
                    {
                        standaloneDuration = duration - childTime;
                        elementDurations.TryRemove(LayoutPassType.Measure, out _);
                    }
                }

                _measureDuration += element is ILayout ? standaloneDuration : duration;

                var tags = new TagList { { "element.type", element.GetType().Name } };
                if (hasChildren)
                {
                    tags.Add("element.childrencount", ((ILayout)element).Count.ToString());
                }

                _measureDurationHistogram.Record(standaloneDuration, tags);
            }
            else
            {
                _measureDuration += duration;
                _measureDurationHistogram.Record(standaloneDuration);
            }

            var elementRef = element is not null ? new WeakReference<object>(element) : null;
            var update = new LayoutUpdate(LayoutPassType.Measure, duration, elementRef, DateTime.UtcNow);

            lock (_historyLock)
            {
                _updateHistory.Add(update);
            }

            PublishLayoutUpdate(update);
        }

        /// <summary>
        /// Records an arrange pass for the specified element, adjusting durations for nested children if applicable.
        /// </summary>
        /// <param name="duration">The duration of the arrange pass in milliseconds.</param>
        /// <param name="element">The element being arranged, or null if not specified.</param>
        public void RecordArrangePass(double duration, object? element = null)
        {
            double standaloneDuration = duration;

            if (element is not null)
            {
                bool hasChildren = element is ILayout layout && layout.Count > 0;
                object? parent = element is IView view ? view.Parent : null;

                if (parent is not null)
                {
                    var parentDurations = _childDurations.GetOrCreateValue(parent);
                    parentDurations.AddOrUpdate(
                        LayoutPassType.Arrange,
                        duration,
                        (key, oldValue) => oldValue + duration);
                }

                if (element is ILayout)
                {
                    if (_childDurations.TryGetValue(element, out var elementDurations) &&
                        elementDurations.TryGetValue(LayoutPassType.Arrange, out var childTime))
                    {
                        standaloneDuration = duration - childTime;
                        elementDurations.TryRemove(LayoutPassType.Arrange, out _);
                    }
                }

                _arrangeDuration += element is ILayout ? standaloneDuration : duration;

                var tags = new TagList { { "element.type", element.GetType().Name } };
                if (hasChildren)
                {
                    tags.Add("element.childrencount", ((ILayout)element).Count.ToString());
                }

                _arrangeDurationHistogram.Record(standaloneDuration, tags);
            }
            else
            {
                _arrangeDuration += duration;
                _arrangeDurationHistogram.Record(standaloneDuration);
            }

            var elementRef = element is not null ? new WeakReference<object>(element) : null;
            var update = new LayoutUpdate(LayoutPassType.Arrange, duration, elementRef, DateTime.UtcNow);

            lock (_historyLock)
            {
                _updateHistory.Add(update);
            }

            PublishLayoutUpdate(update);
        }

        /// <summary>
        /// Publishes a layout update to all subscribed callbacks.
        /// </summary>
        /// <param name="update">The layout update to publish.</param>
        private void PublishLayoutUpdate(LayoutUpdate update)
        {
            List<WeakReference<Action<LayoutUpdate>>> subscribersSnapshot;
            lock (_subscriberLock)
            {
                subscribersSnapshot = new List<WeakReference<Action<LayoutUpdate>>>(_subscribers);
            }

            var aliveSubscribers = new List<WeakReference<Action<LayoutUpdate>>>();
            foreach (var subscriberRef in subscribersSnapshot)
            {
                if (subscriberRef.TryGetTarget(out var subscriber))
                {
                    aliveSubscribers.Add(subscriberRef);
                    try
                    {
                        subscriber.Invoke(update);
                    }
                    catch
                    {
                        // Swallow exceptions from subscribers.
                    }
                }
            }

            // Clean up dead references
            lock (_subscriberLock)
            {
                _subscribers.Clear();
                _subscribers.AddRange(aliveSubscribers);
            }
        }
    }
}