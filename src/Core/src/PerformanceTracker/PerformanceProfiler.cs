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
	/// Starts performance tracking for a given category and optional identifier.
	/// </summary>
	/// <param name="category">The category of operation being tracked (e.g. <see cref="PerformanceCategory.LayoutMeasure"/>).</param>
	/// <param name="id">An optional identifier for the specific element being tracked (e.g. element name).</param>
	/// <returns>A <see cref="PerformanceTracker"/> struct that must be explicitly stopped via <see cref="PerformanceTracker.Stop"/>.</returns>
	public static PerformanceTracker Start(PerformanceCategory category, string id = null)
	{
		if (Layout is null)
			return default;

		return new PerformanceTracker(category, id);
	}

	/// <summary>
	/// Represents a running performance tracking operation. Must call <see cref="Stop"/> to record the timing.
	/// </summary>
	internal readonly struct PerformanceTracker
	{
		readonly PerformanceCategory _category;
		readonly string _element;
		readonly long _startTimestamp;
		readonly bool _isActive;

		/// <summary>
		/// Creates a new performance tracker for the given category and identifier.
		/// </summary>
		/// <param name="category">The category of the operation being tracked.</param>
		/// <param name="element">The optional identifier (e.g. element name or operation label).</param>
		public PerformanceTracker(PerformanceCategory category, string element)
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
	}
}