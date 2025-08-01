using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Performance;
	
/// <summary>
/// Indicates whether this layout update corresponds to a Measure or Arrange pass.
/// </summary>
internal enum LayoutPassType
{
	Measure,
	Arrange
}

/// <summary>
/// Represents a single layout‐engine update event (either a Measure or Arrange pass).
/// </summary>
internal class LayoutUpdate
{
	/// <summary>
	/// The kind of layout pass (Measure or Arrange).
	/// </summary>
	public LayoutPassType PassType { get; set; }

	/// <summary>
	/// Total elapsed time for this pass, in milliseconds.
	/// </summary>
	public double TotalTime { get; set; }

	/// <summary>
	/// Weak reference to the element (e.g. "StackLayout", "Grid") on which the pass was performed.
	/// </summary>
	public WeakReference<object>? Element { get; set; }

	/// <summary>
	/// UTC timestamp when this layout pass was completed.
	/// </summary>
	public DateTime TimestampUtc { get; set; }

	/// <summary>
	/// Initializes a new instance of LayoutUpdate.
	/// </summary>
	/// <param name="passType">Measure or Arrange.</param>
	/// <param name="totalTime">Elapsed time in milliseconds.</param>
	/// <param name="element">Weak reference to the element, or null.</param>
	/// <param name="timestampUtc">Optional override for timestamp; otherwise uses UtcNow.</param>
	public LayoutUpdate(
		LayoutPassType passType,
		double totalTime,
		WeakReference<object>? element,
		DateTime? timestampUtc = null)
	{
		PassType = passType;
		TotalTime = totalTime;
		Element = element;
		TimestampUtc = timestampUtc ?? DateTime.UtcNow;
	}
	
	/// <summary>
	/// Attempts to get the target element if it's still alive.
	/// </summary>
	/// <param name="target">The target element if available; otherwise null.</param>
	/// <returns>True if the element is available, false if garbage collected or not set.</returns>
	public bool TryGetElement([NotNullWhen(true)] out object? target)
	{
		target = null;
		return Element?.TryGetTarget(out target) == true;
	}
}