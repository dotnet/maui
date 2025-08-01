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
		TimestampUtc = DateTime.UtcNow
	};
	
	/// <summary>
	/// Returns a string representation of the performance update.
	/// </summary>
	public override string ToString()
	{
		string layoutEntries;

		if (Layout is not null)
		{
			var lines = new List<string>();
			foreach (var layoutUpdate in Layout)
			{
				lines.Add("  " + layoutUpdate);
			}
			layoutEntries = lines.Count > 0 ? string.Join(Environment.NewLine, lines) : "  (no layout updates)";
		}
		else
		{
			layoutEntries = "  (no layout updates)";
		}

		return $"Performance Update at {TimestampUtc:u}\nLayout Updates:\n{layoutEntries}";
	}

}