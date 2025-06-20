namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents configuration options for performance warnings.
/// Allows enabling or disabling warnings, setting a minimum severity level, and defining threshold values.
/// </summary>
public class WarningOptions
{
	/// <summary>
	/// Determines whether performance warnings are enabled.
	/// If set to <c>true</c>, warnings will be generated when metrics exceed their thresholds.
	/// </summary>
	public bool Enable { get; set; } = true;

	/// <summary>
	/// Specifies the minimum warning level required for a performance alert to be triggered.
	/// Warnings below this level will not be reported.
	/// </summary>
	public PerformanceWarningLevel MinimumLevel { get; set; } = PerformanceWarningLevel.Warning;

	/// <summary>
	/// Defines the threshold limits for various performance metrics.
	/// These values determine when performance warnings should be raised.
	/// </summary>
	public PerformanceThresholds Thresholds { get; set; } = new();
}