using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Defines an interface for tracking the performance of layout operations such as
/// measure and arrange passes within the layout system.
/// </summary>
internal interface ILayoutPerformanceTracker
{
	/// <summary>
	/// Configures the layout tracking behavior using the provided options.
	/// </summary>
	/// <param name="options">The layout tracking options to apply.</param>
	void Configure(LayoutTrackingOptions options);

	/// <summary>
	/// Retrieves current aggregated layout performance statistics.
	/// </summary>
	/// <returns>A <see cref="LayoutStats"/> object containing layout metrics.</returns>
	LayoutStats GetStats();

	/// <summary>
	/// Subscribes to real-time layout update notifications, such as measure and arrange passes.
	/// </summary>
	/// <param name="callback">A callback that receives <see cref="LayoutUpdate"/> data.</param>
	void SubscribeToLayoutUpdates(Action<LayoutUpdate> callback);

	/// <summary>
	/// Records a measure pass with the specified duration and optional element type.
	/// </summary>
	/// <param name="duration">The duration of the measure pass in milliseconds.</param>
	/// <param name="element">An optional string representing the type of element measured.</param>
	void RecordMeasurePass(long duration, string? element = null);

	/// <summary>
	/// Records an arrange pass with the specified duration and optional element type.
	/// </summary>
	/// <param name="duration">The duration of the arrange pass in milliseconds.</param>
	/// <param name="element">An optional string representing the type of element arranged.</param>
	void RecordArrangePass(long duration, string? element = null);
}