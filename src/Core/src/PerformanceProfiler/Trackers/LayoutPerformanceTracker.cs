using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Maui;

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

        readonly List<Action<LayoutUpdate>> _subscribers = new();
        readonly object _subscriberLock = new();

        // Tracks cumulative duration spent on direct children for each element and pass type
        readonly ConcurrentDictionary<(object Element, LayoutPassType PassType), double> _childDurations = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutPerformanceTracker"/> class with the specified meter for metrics collection.
        /// </summary>
        /// <param name="meter">The meter used to create histograms for tracking measure and arrange durations.</param>
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
        /// Subscribes to real-time layout update events for measure and arrange passes.
        /// </summary>
        /// <param name="callback">The callback to invoke when a layout update occurs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is null.</exception>
        public void SubscribeToLayoutUpdates(Action<LayoutUpdate> callback)
        {
            if (callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            lock (_subscriberLock)
            {
                _subscribers.Add(callback);
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

            // Check if element has children and get parent
            bool hasChildren = element is ILayout layout && layout.Count > 0;
            object? parent = element is IView view ? view.Parent : null;

            // If element has a parent, accumulate child duration
            if (parent is not null)
            {
                _childDurations.AddOrUpdate(
                    (parent, LayoutPassType.Measure),
                    duration,
                    (key, oldValue) => oldValue + duration);
            }

            // For ILayout elements, subtract child durations to get standalone duration
            if (element is ILayout)
            {
                standaloneDuration = _childDurations.TryGetValue((element, LayoutPassType.Measure), out var childTime)
                    ? duration - childTime
                    : duration;
                _childDurations.TryRemove((element, LayoutPassType.Measure), out _);
            }

            // Update fields with standalone duration for ILayout, total duration otherwise
            _measureDuration += element is ILayout ? standaloneDuration : duration;

            // Record metrics
            var tags = new TagList();
            if (element is not null)
            {
                tags.Add("element.type", element.GetType().Name);
                if (hasChildren)
                {
                    tags.Add("element.childrencount", ((ILayout)element).Count.ToString());
                }
            }

            _measureDurationHistogram.Record(standaloneDuration, tags);

            var update = new LayoutUpdate(LayoutPassType.Measure, duration, element ?? string.Empty, DateTime.UtcNow);

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

            // Check if element has children and get parent
            bool hasChildren = element is ILayout layout && layout.Count > 0;
            object? parent = element is IView view ? view.Parent : null;
            
            // If element has a parent, accumulate child duration
            if (parent is not null)
            {
                _childDurations.AddOrUpdate(
                    (parent, LayoutPassType.Arrange),
                    duration,
                    (key, oldValue) => oldValue + duration);
            }

            // For ILayout elements, subtract child durations to get standalone duration
            if (element is ILayout)
            {
                standaloneDuration = _childDurations.TryGetValue((element, LayoutPassType.Arrange), out var childTime)
                    ? duration - childTime
                    : duration;
                _childDurations.TryRemove((element, LayoutPassType.Arrange), out _);
            }

            // Update fields with standalone duration for ILayout, total duration otherwise
            _arrangeDuration += element is ILayout ? standaloneDuration : duration;

            // Record metrics
            var tags = new TagList();
            if (element is not null)
            {
                tags.Add("element.type", element.GetType().Name);
                if (hasChildren)
                {
                    tags.Add("element.childrencount", ((ILayout)element).Count.ToString());
                }
            }

            _arrangeDurationHistogram.Record(standaloneDuration, tags);

            var update = new LayoutUpdate(LayoutPassType.Arrange, duration, element ?? string.Empty, DateTime.UtcNow);

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