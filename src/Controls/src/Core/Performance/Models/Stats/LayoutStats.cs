namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents aggregated performance statistics for layout operations,
/// including measure and arrange passes.
/// </summary>
internal class LayoutStats
{
	/// <summary>
	/// Gets or sets the total number of measure passes performed.
	/// </summary>
	public long MeasurePassCount { get; set; }

	/// <summary>
	/// Gets or sets the average duration (in milliseconds) of measure passes.
	/// </summary>
	public double AverageMeasureDuration { get; set; }

	/// <summary>
	/// Gets or sets the peak (maximum) duration (in milliseconds) of any single measure pass.
	/// </summary>
	public double PeakMeasureDuration { get; set; }

	/// <summary>
	/// Gets or sets the total number of arrange passes performed.
	/// </summary>
	public long ArrangePassCount { get; set; }

	/// <summary>
	/// Gets or sets the average duration (in milliseconds) of arrange passes.
	/// </summary>
	public double AverageArrangeDuration { get; set; }

	/// <summary>
	/// Gets or sets the peak (maximum) duration (in milliseconds) of any single arrange pass.
	/// </summary>
	public double PeakArrangeDuration { get; set; }
}