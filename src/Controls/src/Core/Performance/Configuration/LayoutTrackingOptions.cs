namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Options to control how layout‐engine performance is tracked.
/// </summary>
public class LayoutTrackingOptions
{
	/// <summary>
	/// If true, the tracker will record each Measure pass.
	/// </summary>
	public bool EnableMeasurePassTracking { get; set; } = true;

	/// <summary>
	/// If true, the tracker will record each Arrange pass.
	/// </summary>
	public bool EnableArrangePassTracking { get; set; } = true;
	
	/// <summary>
	/// If true, the tracker will tag each recorded pass with the element type.
	/// </summary>
	public bool TrackPerElement { get; set; } = true;
}