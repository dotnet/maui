using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Defines an interface for tracking the performance of scrolling operations such as
/// frame rate, frame time consistency, and resource usage during scrolling.
/// </summary>
internal interface IScrollingPerformanceTracker
{
	/// <summary>
	/// Retrieves current scrolling performance statistics.
	/// </summary>
	/// <returns>A <see cref="ScrollingStats"/> object containing scrolling metrics.</returns>
	ScrollingStats GetStats();

	/// <summary>
	/// Retrieves the history of scrolling updates, optionally filtered by element.
	/// </summary>
	/// <param name="element">Optional element to filter by instance.</param>
	/// <returns>A list of <see cref="ScrollingUpdate"/> records matching the filter criteria.</returns>
	IEnumerable<ScrollingUpdate> GetHistory(object? element = null);
	
	/// <summary>
	/// Subscribes to real-time scrolling update notifications.
	/// </summary>
	/// <param name="callback">A callback that receives <see cref="ScrollingUpdate"/> data.</param>
	void SubscribeToScrollingUpdates(Action<ScrollingUpdate> callback);
	
	/// <summary>
	/// Records scrolling time and tracks dropped frames, outputting debug information.
	/// </summary>
	/// <param name="duration">The scrolling duration in milliseconds.</param>
	/// <param name="element">An optional object representing the scrolling element.</param>
	void RecordScrollingTime(double duration, object? element = null);
}