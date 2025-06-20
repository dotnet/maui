#nullable disable
using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents a performance warning triggered when a metric exceeds its configured threshold.
/// Provides details about the affected category, severity level, and recommended actions.
/// </summary>
public class PerformanceWarning
{
	/// <summary>
	/// The category of the performance issue (e.g., "Rendering", "Memory", "Layout").
	/// </summary>
	public string Category { get; set; }

	/// <summary>
	/// The specific metric that exceeded its threshold (e.g., "Frame Time", "Memory Usage").
	/// </summary>
	public string Metric { get; set; }

	/// <summary>
	/// A message describing the performance warning in detail.
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// A recommendation for resolving the performance issue (e.g., "Reduce UI complexity", "Optimize memory allocation").
	/// </summary>
	public string Recommendation { get; set; }

	/// <summary>
	/// The severity level of the performance warning.
	/// </summary>
	public PerformanceWarningLevel Level { get; set; }

	/// <summary>
	/// The timestamp when the performance warning was generated.
	/// </summary>
	public DateTime Timestamp { get; set; }

	/// <summary>
	/// The current value of the metric at the time of the warning.
	/// </summary>
	public object CurrentValue { get; set; }

	/// <summary>
	/// The threshold value that triggered the warning.
	/// </summary>
	public object ThresholdValue { get; set; }

	/// <summary>
	/// The unit of measurement for the metric (e.g., "ms", "MB").
	/// </summary>
	public string Unit { get; set; }
}