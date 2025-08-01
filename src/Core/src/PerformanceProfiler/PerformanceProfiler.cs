#nullable disable
using System;
using System.Diagnostics;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Provides performance tracking for various runtime operations such as layout passes, image loading, etc.
/// </summary>
internal static class PerformanceProfiler
{
	/// <summary>
	/// Gets the layout performance tracker used to monitor layout-related operations.
	/// </summary>
	public static ILayoutPerformanceTracker Layout { get; private set; }

	/// <summary>
	/// Initializes the performance profiler with a performance tracker.
	/// </summary>
	/// <param name="layout">An instance of <see cref="ILayoutPerformanceTracker"/> to be used for tracking layout performance.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="layout"/> is null.</exception>
	public static void Initialize(ILayoutPerformanceTracker layout)
	{
		Layout = layout ?? throw new ArgumentNullException(nameof(layout));
	}

	/// <summary>
	/// Collects and returns performance statistics related to tracker operations.
	/// </summary>
	/// <returns>
	/// A <see cref="PerformanceStats"/> object containing tracker metrics and the timestamp at which the data was collected.
	/// </returns>
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
	/// Retrieves the current performance update history for the layout tracker, optionally filtered by the specified element.
	/// </summary>
	/// <param name="element">An optional element to filter updates by instance. If null, all updates are returned.</param>
	/// <returns>
	/// A <see cref="PerformanceUpdate"/> instance containing performance-related history. 
	/// Returns an empty update object if the trackers are not initialized.
	/// </returns>
	public static PerformanceUpdate GetHistory(object element = null)
	{
		if (Layout is null)
			return PerformanceUpdate.Empty;
		
		var layout = Layout.GetHistory(element);

		var performanceUpdate = new PerformanceUpdate
		{
			Layout = layout,
			TimestampUtc = DateTime.UtcNow
		};
		
		return performanceUpdate;
	}
	
	/// <summary>
	/// Subscribe to receive real-time LayoutUpdate events (Measure or Arrange).
	/// </summary>
	/// <param name="callback">The callback to invoke when a layout update occurs.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
	public static void SubscribeToUpdates(Action<LayoutUpdate> callback)
	{
		if (callback == null)
			throw new ArgumentNullException(nameof(callback));

		Layout?.SubscribeToLayoutUpdates(callback);
	}

	/// <summary>
	/// Starts performance tracking for a given category and optional element.
	/// </summary>
	/// <param name="category">The category of operation being tracked (e.g. <see cref="PerformanceCategory.LayoutMeasure"/>).</param>
	/// <param name="element">An optional object identifying the specific element being tracked.</param>
	/// <returns>A <see cref="PerformanceTracker"/> struct that must be explicitly stopped via <see cref="PerformanceTracker.Stop"/>.</returns>
	public static PerformanceTracker Start(PerformanceCategory category, object element = null)
	{
		if (Layout is null)
			return default;

		return new PerformanceTracker(category, element);
	}

	/// <summary>
	/// Represents a running performance tracking operation. Must call <see cref="Stop"/> to record the timing.
	/// </summary>
	internal readonly struct PerformanceTracker : IDisposable
	{
		readonly PerformanceCategory _category;
		readonly object _element;
		readonly long _startTimestamp;
		readonly bool _isActive;

		/// <summary>
		/// Creates a new performance tracker for the given category and identifier.
		/// </summary>
		/// <param name="category">The category of the operation being tracked.</param>
		/// <param name="element">An optional object identifying the specific element being tracked.</param>
		public PerformanceTracker(PerformanceCategory category, object element)
		{
			_category = category;
			_element = element;
			_startTimestamp = Stopwatch.GetTimestamp();
			_isActive = true;
		}

		/// <summary>
		/// Stops the tracking operation and records the duration with the appropriate tracker.
		/// </summary>
		public void Stop()
		{
			if (!_isActive || Layout is null)
				return;

			var elapsed = (Stopwatch.GetTimestamp() - _startTimestamp) * 1000.0 / Stopwatch.Frequency;

			switch (_category)
			{
				case PerformanceCategory.LayoutMeasure:
					Layout.RecordMeasurePass(elapsed, _element);
					break;

				case PerformanceCategory.LayoutArrange:
					Layout.RecordArrangePass(elapsed, _element);
					break;

				default:
					Debug.WriteLine($"[Performance Profiler] {_category} took {elapsed:0.00} ms.");
					break;
			}
		}
		
		/// <summary>
		/// Disposes the performance tracker by stopping the tracking operation.
		/// </summary>
		public void Dispose()
		{
			Stop();
		}
	}
}