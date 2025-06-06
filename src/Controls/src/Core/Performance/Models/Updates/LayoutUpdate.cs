using System;

namespace Microsoft.Maui.Controls.Performance
{
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
		/// Total elapsed time for this pass, in nanoseconds.
		/// (Clients can divide by 1,000,000.0 to get milliseconds.)
		/// </summary>
		public long TotalTime { get; set; }

		/// <summary>
		/// The element type (e.g. "StackLayout", "Grid") on which the pass was performed.
		/// </summary>
		public string ElementType { get; set; }

		/// <summary>
		/// UTC timestamp when this layout pass was completed.
		/// </summary>
		public DateTime TimestampUtc { get; set; }

		/// <summary>
		/// Initializes a new instance of LayoutUpdate.
		/// </summary>
		/// <param name="passType">Measure or Arrange.</param>
		/// <param name="totalTimeNanoseconds">Elapsed time in nanoseconds.</param>
		/// <param name="elementType">Type/name of the element.</param>
		/// <param name="timestampUtc">Optional override for timestamp; otherwise uses UtcNow.</param>
		public LayoutUpdate(
			LayoutPassType passType,
			long totalTimeNanoseconds,
			string elementType,
			DateTime? timestampUtc = null)
		{
			PassType = passType;
			TotalTime = totalTimeNanoseconds;
			ElementType = elementType;
			TimestampUtc = timestampUtc ?? DateTime.UtcNow;
		}
	}
}