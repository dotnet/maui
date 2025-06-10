namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents aggregated performance statistics for layout operations,
/// including measure and arrange passes.
/// </summary>
public class LayoutStats
{
	/// <summary>
	/// Gets or sets the total number of measure passes performed.
	/// </summary>
	public long MeasurePassCount { get; set; }

	/// <summary>
	/// Gets or sets the duration (in milliseconds) of measure passes.
	/// </summary>
	public double MeasureDuration { get; set; }
	
	/// <summary>
	/// Gets or sets the total number of arrange passes performed.
	/// </summary>
	public long ArrangePassCount { get; set; }

	/// <summary>
	/// Gets or sets the duration (in milliseconds) of arrange passes.
	/// </summary>
	public double ArrangeDuration { get; set; }
}