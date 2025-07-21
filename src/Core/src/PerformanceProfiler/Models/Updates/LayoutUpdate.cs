using System;

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
	/// The element (e.g. "StackLayout", "Grid") on which the pass was performed.
	/// </summary>
	public object Element { get; set; }

	/// <summary>
	/// UTC timestamp when this layout pass was completed.
	/// </summary>
	public DateTime TimestampUtc { get; set; }

	/// <summary>
	/// Initializes a new instance of LayoutUpdate.
	/// </summary>
	/// <param name="passType">Measure or Arrange.</param>
	/// <param name="totalTime">Elapsed time in milliseconds.</param>
	/// <param name="element">The element on which the pass was performed.</param>
	/// <param name="timestampUtc">Optional override for timestamp; otherwise uses UtcNow.</param>
	public LayoutUpdate(
		LayoutPassType passType,
		double totalTime,
		object element,
		DateTime? timestampUtc = null)
	{
		PassType = passType;
		TotalTime = totalTime;
		Element = element;
		TimestampUtc = timestampUtc ?? DateTime.UtcNow;
	}
}