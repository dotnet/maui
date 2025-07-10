#nullable disable
using System;
using System.Diagnostics;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Provides access to performance tracking.
/// </summary>
internal static class PerformanceProfiler
{
	static readonly IDisposable Untracked = new ActionDisposable(() => { /* nothing */ });

	/// <summary>
	/// Gets the layout performance tracker responsible for monitoring
	/// layout-related operations such as measure and arrange passes.
	/// </summary>
	/// <remarks>
	/// This tracker provides insights into UI rendering performance, helping detect 
	/// inefficiencies in layout calculations and arrangement processes.
	/// </remarks>
	public static ILayoutPerformanceTracker Layout { get; private set; }

	/// <summary>
	/// Initializes the performance profiler with the given tracker implementations.
	/// </summary>
	/// <param name="layout">An instance of <see cref="ILayoutPerformanceTracker"/> to be used for tracking performance.</param>
	public static void Initialize(ILayoutPerformanceTracker layout)
	{
		Layout = layout ?? throw new ArgumentNullException(nameof(layout));
	}

	/// <summary>
	/// Collects and returns performance statistics related to layout operations.
	/// </summary>
	/// <returns>
	/// A <see cref="PerformanceStats"/> object that contains metrics collected
	/// from the performance trackers and a timestamp representing the moment
	/// the stats were retrieved.
	/// </returns>
	/// <remarks>
	/// This method is useful for diagnostics, telemetry, or logging purposes when evaluating
	/// rendering performance over time. The <see cref="PerformanceStats"/> returned includes
	/// details such as measure/arrange counts or durations, and when the snapshot was taken.
	/// </remarks>
	public static PerformanceStats GetStats()
	{
		if (Layout is null)
			return PerformanceStats.Empty;
		
		return new PerformanceStats
		{
			Layout = Layout.GetStats(),
			TimestampUtc = DateTime.UtcNow
		};
	}

	/// <summary>
	/// Starts timing a layout-measure pass. Dispose the returned scope to stop and record.
	/// </summary>
	/// <param name="element">
	/// Optional element name or type for per-element tracking.
	/// </param>
	/// <returns>
	/// An <see cref="IDisposable"/> which, when disposed, stops timing
	/// and records the duration with the layout tracker.
	/// </returns>
	public static IDisposable TrackMeasure(string element = null)
	{
		if (Layout is null)
			return Untracked;

		var sw = Stopwatch.StartNew();
		return new ActionDisposable(() =>
		{
			sw.Stop();
			Layout.RecordMeasurePass(sw.Elapsed.TotalMilliseconds, element);
		});
	}

	/// <summary>
	/// Starts timing a layout-arrange pass. Dispose the returned scope to stop and record.
	/// </summary>
	/// <param name="element">
	/// Optional element name or type for per-element tracking.
	/// </param>
	/// <returns>
	/// An <see cref="IDisposable"/> which, when disposed, stops timing
	/// and records the duration with the layout tracker.
	/// </returns>
	public static IDisposable TrackArrange(string element = null)
	{
		if (Layout is null)
			return Untracked;

		var sw = Stopwatch.StartNew();
		return new ActionDisposable(() =>
		{
			sw.Stop();
			Layout.RecordArrangePass(sw.Elapsed.TotalMilliseconds, element);
		});
	}
}