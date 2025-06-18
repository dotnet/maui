using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Represents performance statistics.
/// </summary>
internal class PerformanceStats
{
	/// <summary>
	/// Gets or sets the image-related performance statistics,
	/// such as load times.
	/// </summary>
	public ImageStats? Image { get; set; }

	/// <summary>
	/// Gets or sets the layout-related performance statistics,
	/// including metrics gathered from measure and arrange passes.
	/// </summary>
	public LayoutStats? Layout { get; set; }

	/// <summary>
	/// Gets or sets the navigation-related performance statistics,
	/// such as timing during page navigations.
	/// </summary>
	public NavigationStats? Navigation { get; set; }
	
	/// <summary>
	/// Gets or sets the UTC timestamp representing when the performance statistics
	/// snapshot was captured.
	/// </summary>
	/// <remarks>
	/// Useful for tracking when metrics were recorded during diagnostics,
	/// logging, or time-based analysis.
	/// </remarks>
	public DateTime TimestampUtc { get; set; }
}