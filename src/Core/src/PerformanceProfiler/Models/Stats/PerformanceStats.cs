using System;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Represents performance statistics.
/// </summary>
internal class PerformanceStats
{
	/// <summary>
	/// Gets or sets the layout-related performance statistics.
	/// </summary>
	public LayoutStats? Layout { get; set; }
	
	/// <summary>
	/// The timestamp (UTC) when the performance update was recorded.
	/// </summary>
	/// <remarks>
	/// Capturing this timestamp allows performance monitoring over time 
	/// and helps correlate performance events.
	/// </remarks>
	public DateTime TimestampUtc { get; set; }

	/// <summary>
	/// Returns a default, empty instance of <see cref="PerformanceStats"/> with no data.
	/// </summary>
	public static PerformanceStats Empty { get; } = new PerformanceStats
	{
		Layout = null,
		TimestampUtc = DateTime.UtcNow
	};
}