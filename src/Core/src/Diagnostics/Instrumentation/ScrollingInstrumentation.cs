using System;
using System.Diagnostics;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Instrumentation for measuring scrolling operations in a view.
/// </summary>
readonly struct ScrollingInstrumentation : IDiagnosticInstrumentation
{
    readonly IView _view;
    readonly Activity? _activity;
    readonly long _startTime;
    readonly string _scrollType;
    readonly double _initialHorizontalOffset;
    readonly double _initialVerticalOffset;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollingInstrumentation"/> struct.
    /// </summary>
    /// <param name="view">The view that is being scrolled.</param>
    /// <param name="scrollType">The type of scroll operation (e.g., "UserGesture", "Programmatic").</param>
    /// <param name="horizontalOffset">The initial horizontal scroll offset.</param>
    /// <param name="verticalOffset">The initial vertical scroll offset.</param>
    public ScrollingInstrumentation(IView view, string scrollType, double horizontalOffset = 0,
	    double verticalOffset = 0)
    {
	    _view = view;
	    _scrollType = string.IsNullOrEmpty(scrollType) ? "Unknown" : scrollType;
	    _initialHorizontalOffset = horizontalOffset;
	    _initialVerticalOffset = verticalOffset;
	    _activity = view.StartDiagnosticActivity($"Scroll.{scrollType}");
	    _startTime = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Disposes the instrumentation and stops the diagnostic activity.
    /// </summary>
    public void Dispose() =>
        _view.StopDiagnostics(_activity, this);

    /// <summary>
    /// Records the stopping of the instrumentation and publishes various metrics.
    /// </summary>
    /// <param name="diagnostics">The <see cref="IDiagnosticsManager"/> instance.</param>
    /// <param name="tagList">The tags associated with the instrumentation.</param>
    public void Stopped(IDiagnosticsManager diagnostics, in TagList tagList)
    {
        var metrics = diagnostics.GetMetrics<ScrollingDiagnosticMetrics>();
        if (metrics is null)
        {
            return;
        }

        try
        {
            // Create extended tag list
            var extendedTagList = BuildExtendedTagList(tagList);

            // Calculate duration
            TimeSpan? duration = _activity?.Duration;
            if (duration is null)
            {
	            var elapsedTicks = Stopwatch.GetTimestamp() - _startTime;
	            duration = new TimeSpan((long)(elapsedTicks * TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency));
            }

            // Record the scroll operation
            metrics.RecordScroll(duration, in extendedTagList);

            // Check for potential jank
            double jankThreshold = 1000.0 / DeviceDisplay.MainDisplayInfo.RefreshRate; // 16.67ms for 60Hz, 8.33ms for 120Hz
            
            if (duration?.TotalMilliseconds > jankThreshold)
            {
	            int droppedFrames = (int)Math.Floor(duration.Value.TotalMilliseconds / jankThreshold);
	            extendedTagList.Add("scroll.droppedFrames", droppedFrames);
	            metrics.RecordJank(in extendedTagList);
            }
        }
        catch
        {
            // Silently catch any exceptions during metrics recording to avoid crashing the app
            // Diagnostics should never affect application stability
        }
    }

    TagList BuildExtendedTagList(in TagList originalTagList)
    {
        // Pre-allocate with estimated capacity to avoid multiple reallocations
        var extendedTagList = new TagList();
        
        // Copy original tags
        foreach (var tag in originalTagList)
        {
            extendedTagList.Add(tag.Key, tag.Value);
        }

        // Add device-specific tags
        extendedTagList.Add("device.platform", DeviceInfo.Platform.ToString());
        extendedTagList.Add("device.model", DeviceInfo.Model);
        
        // Add scroll-specific tags
        extendedTagList.Add("scroll.type", _scrollType);
        extendedTagList.Add("control.type", GetControlType(_view));

        // Add scroll distance and direction
        if (_view is IScrollView scrollView)
        {
            var (horizontalDistance, verticalDistance, totalDistance) = CalculateScrollDistances(scrollView);
            
            extendedTagList.Add("scroll.distance", (int)Math.Round(totalDistance));
            extendedTagList.Add("scroll.direction", GetScrollDirection(horizontalDistance, verticalDistance, scrollView.Orientation));
        }
        else
        {
            extendedTagList.Add("scroll.distance", 0);
            extendedTagList.Add("scroll.direction", "Unknown");
        }

        return extendedTagList;
    }
	
    (double horizontal, double vertical, double total) CalculateScrollDistances(IScrollView scrollView)
    {
        var horizontalDistance = Math.Abs(scrollView.HorizontalOffset - _initialHorizontalOffset);
        var verticalDistance = Math.Abs(scrollView.VerticalOffset - _initialVerticalOffset);
        var totalDistance = Math.Sqrt(horizontalDistance * horizontalDistance + verticalDistance * verticalDistance);
        
        return (horizontalDistance, verticalDistance, totalDistance);
    }

    static string GetControlType(IView view)
    {
        var typeName = view.GetType().Name;
        
  #if !NETSTANDARD      
        if (typeName.Contains("ScrollView", StringComparison.OrdinalIgnoreCase))
        {
	        return "ScrollView";
        }
        
        if (typeName.Contains("CarouselView", StringComparison.OrdinalIgnoreCase))
        {
	        return "CarouselView";
        }
        
        if (typeName.Contains("CollectionView", StringComparison.OrdinalIgnoreCase))
        {
	        return "CollectionView";
        }
#endif
        return typeName;
    }

    static string GetScrollDirection(double horizontalDistance, double verticalDistance, ScrollOrientation orientation)
    {
	    // Use a small threshold to avoid noise from minimal movements
	    const double minMovementThreshold = 1.0;

	    if (horizontalDistance > verticalDistance && horizontalDistance > minMovementThreshold)
	    {
		    return "Horizontal";
	    }
	    else if (verticalDistance > horizontalDistance && verticalDistance > minMovementThreshold)
	    {
		    return "Vertical";
	    }
	    else if (horizontalDistance > minMovementThreshold && verticalDistance > minMovementThreshold)
	    {
		    return "Both";
	    }

	    // Fallback to orientation if no significant movement detected
	    return orientation switch
	    {
		    ScrollOrientation.Horizontal => "Horizontal",
		    ScrollOrientation.Vertical => "Vertical",
		    ScrollOrientation.Both => "Both",
		    _ => "Unknown"
	    };
    }
}