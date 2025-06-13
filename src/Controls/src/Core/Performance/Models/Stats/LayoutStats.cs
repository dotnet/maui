namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents aggregated performance statistics for layout operations,
/// including measure and arrange passes.
/// </summary>
public class LayoutStats
{
	/// <summary>
	/// Gets or sets the duration (in milliseconds) of measure passes.
	/// </summary>
	public double MeasureDuration { get; set; }

	/// <summary>
	/// Gets or sets the duration (in milliseconds) of arrange passes.
	/// </summary>
	public double ArrangeDuration { get; set; }
}