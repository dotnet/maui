using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents performance thresholds for different aspects of performance.
/// </summary>
public class PerformanceThresholds
{
	/// <summary>
	/// Gets or sets the layout performance thresholds, which define acceptable limits for various layout metrics.
	/// </summary>
	public LayoutThresholds Layout { get; set; } = new();
}