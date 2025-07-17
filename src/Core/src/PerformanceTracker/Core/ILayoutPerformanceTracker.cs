namespace Microsoft.Maui.Performance;

/// <summary>
/// Defines an interface for tracking the performance of layout operations such as
/// measure and arrange passes within the layout system.
/// </summary>
internal interface ILayoutPerformanceTracker
{
	/// <summary>
	/// Retrieves current aggregated layout performance statistics.
	/// </summary>
	/// <returns>A <see cref="LayoutStats"/> object containing layout metrics.</returns>
	LayoutStats GetStats();
	
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