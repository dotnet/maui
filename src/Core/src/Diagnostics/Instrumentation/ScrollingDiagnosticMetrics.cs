using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Defines diagnostic metrics for scrolling operations, including scroll count, duration, velocity, and jank detection.
/// </summary>
internal class ScrollingDiagnosticMetrics : IDiagnosticMetrics
{
    double _currentVelocity;

    /// <summary>
    /// Gets the counter metric for the number of scroll operations performed.
    /// </summary>
    public Counter<int>? ScrollingCounter { get; private set; }

    /// <summary>
    /// Gets the histogram metric for the duration of scroll operations in nanoseconds.
    /// </summary>
    public Histogram<int>? ScrollingDuration { get; private set; }

    /// <summary>
    /// Gets the observable gauge metric for the current scroll velocity in pixels per second.
    /// </summary>
    public ObservableGauge<double>? ScrollingVelocity { get; private set; }

    /// <summary>
    /// Gets the counter metric for the number of jank events during scrolling.
    /// </summary>
    public Counter<int>? ScrollingJank { get; private set; }

    /// <inheritdoc/>
    public void Create(Meter meter)
    {
       ScrollingCounter = meter.CreateCounter<int>("maui.scrolling.count", "{scrolls}", "The number of scrolling operations.");
       ScrollingDuration = meter.CreateHistogram<int>("maui.scrolling.duration", "ns", "The duration of scrolling operations.");
       ScrollingVelocity = meter.CreateObservableGauge<double>("maui.scrolling.velocity", () => _currentVelocity,
	       "pixels/s", "The current scroll velocity in pixels per second.");
       ScrollingJank = meter.CreateCounter<int>("maui.scrolling.jank", "{janks}", "The number of jank events during scrolling.");
    }

    /// <summary>
    /// Records a scroll operation with the specified duration and associated tags.
    /// </summary>
    /// <param name="duration">The duration of the scroll operation.</param>
    /// <param name="tagList">The tags associated with the scroll operation.</param>
    public void RecordScroll(TimeSpan? duration, in TagList tagList)
    {
       ScrollingCounter?.Add(1, tagList);

       if (duration is not null)
       {
#if NET9_0_OR_GREATER
          ScrollingDuration?.Record((int)duration.Value.TotalNanoseconds, tagList);
#else
          ScrollingDuration?.Record((int)(duration.Value.TotalMilliseconds * 1_000_000), tagList);
#endif
       }
    }

    /// <summary>
    /// Records a jank event during scrolling with the associated tags.
    /// </summary>
    /// <param name="tagList">The tags associated with the jank event.</param>
    public void RecordJank(in TagList tagList)
    {
       ScrollingJank?.Add(1, tagList);
    }

    /// <summary>
    /// Records a scroll operation with velocity calculation.
    /// </summary>
    /// <param name="duration">The duration of the scroll operation.</param>
    /// <param name="distance">The total distance scrolled in pixels.</param>
    /// <param name="tagList">The tags associated with the scroll operation.</param>
    public void RecordScroll(TimeSpan? duration, double distance, in TagList tagList)
    {
        ScrollingCounter?.Add(1, tagList);

        if (duration is not null)
        {
            // Calculate and update velocity
            var durationInSeconds = duration.Value.TotalSeconds;
            if (durationInSeconds > 0)
            {
                _currentVelocity = distance / durationInSeconds;
            }

#if NET9_0_OR_GREATER
            ScrollingDuration?.Record((int)duration.Value.TotalNanoseconds, tagList);
#else
            ScrollingDuration?.Record((int)(duration.Value.TotalMilliseconds * 1_000_000), tagList);
#endif
        }
    }
}