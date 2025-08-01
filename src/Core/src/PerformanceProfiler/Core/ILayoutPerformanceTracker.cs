using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Defines an interface for tracking the performance of layout operations such as
/// measure and arrange passes within the layout system.
/// </summary>
internal interface ILayoutPerformanceTracker
{
	/// <summary>
	/// Retrieves current layout performance statistics.
	/// </summary>
	/// <returns>A <see cref="LayoutStats"/> object containing layout metrics.</returns>
	LayoutStats GetStats();

	/// <summary>
	/// Retrieves the history of layout updates, optionally filtered by element.
	/// </summary>
	/// <param name="element">Optional element to filter by instance.</param>
	/// <returns>A list of <see cref="LayoutUpdate"/> records matching the filter criteria.</returns>
	/// <exception cref="ArgumentException">Thrown if both element and elementType are provided.</exception>
	IEnumerable<LayoutUpdate> GetHistory(object? element = null);
	
	/// <summary>
	/// Subscribes to real-time layout update notifications, such as measure and arrange passes.
	/// </summary>
	/// <param name="callback">A callback that receives <see cref="LayoutUpdate"/> data.</param>
	void SubscribeToLayoutUpdates(Action<LayoutUpdate> callback);
	
	/// <summary>
	/// Records a measure pass with the specified duration and optional element type.
	/// </summary>
	/// <param name="duration">The duration of the measure pass in milliseconds.</param>
	/// <param name="element">An optional object representing the element measured.</param>
	void RecordMeasurePass(double duration, object? element = null);

	/// <summary>
	/// Records an arrange pass with the specified duration and optional element type.
	/// </summary>
	/// <param name="duration">The duration of the arrange pass in milliseconds.</param>
	/// <param name="element">An optional object representing the element arranged.</param>
	void RecordArrangePass(double duration, object? element = null);
}