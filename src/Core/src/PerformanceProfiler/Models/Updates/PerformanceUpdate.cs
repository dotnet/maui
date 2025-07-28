using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Represents a collection of performance update events across multiple trackers.
/// </summary>
internal class PerformanceUpdate
{
	/// <summary>
	/// A collection of layout-related performance updates.
	/// </summary>
	public IEnumerable<LayoutUpdate>? Layout { get; set; } = new List<LayoutUpdate>();
	
	/// <summary>
	/// Gets or sets a collection of scrolling-related performance updates.
	/// </summary>
	public IEnumerable<ScrollingUpdate>? Scrolling { get; set; } = new List<ScrollingUpdate>();
	
	/// <summary>
	/// The timestamp (UTC) when the performance update was recorded.
	/// </summary>
	/// <remarks>
	/// Capturing this timestamp allows performance monitoring over time 
	/// and helps correlate performance events.
	/// </remarks>
	public DateTime TimestampUtc { get; set; }
	
	/// <summary>
	/// Returns a default, empty instance of <see cref="PerformanceUpdate"/> with no data.
	/// </summary>
	public static PerformanceUpdate Empty { get; } = new PerformanceUpdate
	{
		Layout = null,
		Scrolling = null,
		TimestampUtc = DateTime.UtcNow
	};
}