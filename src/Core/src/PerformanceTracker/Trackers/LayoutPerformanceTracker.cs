using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Performance
{
	internal class LayoutPerformanceTracker : ILayoutPerformanceTracker
	{
		readonly Histogram<double> _measureDurationHistogram;
		readonly Histogram<double> _arrangeDurationHistogram;
		
		double _measureDuration;
		double _arrangeDuration;
		
		public LayoutPerformanceTracker(Meter meter)
		{
			_measureDurationHistogram = meter.CreateHistogram<double>(
				"maui.layout.measure.duration",
				unit: "ms",
				description: "Duration of layout measure passes");

			_arrangeDurationHistogram = meter.CreateHistogram<double>(
				"maui.layout.arrange.duration",
				unit: "ms",
				description: "Duration of layout arrange passes");
		}
		
		/// <summary>
		/// Gets the layout performance statistics.
		/// </summary>
		/// <returns>A <see cref="LayoutStats"/> object containing measure and arrange pass data.</returns>
		public LayoutStats GetStats()
		{
			return new LayoutStats
			{
				MeasureDuration = _measureDuration,
				ArrangeDuration = _arrangeDuration,
			};
		}

		/// <summary>
		/// Record a Measure pass of the layout engine.
		/// </summary>
		public void RecordMeasurePass(double duration, string? element = null)
		{
			// Update fields
			_measureDuration = duration;

			// Record metrics
			var tags = new TagList();
			if (!string.IsNullOrEmpty(element))
			{
				tags.Add("element.type", element);
			}

			_measureDurationHistogram.Record(duration, tags);
		}

		/// <summary>
		/// Record an Arrange pass of the layout engine.
		/// </summary>
		public void RecordArrangePass(double duration, string? element = null)
		{
			// Update fields
			_arrangeDuration = duration;

			// Record metrics
			var tags = new TagList();
			if (!string.IsNullOrEmpty(element))
			{
				tags.Add("element.type", element);
			}

			_arrangeDurationHistogram.Record(duration, tags);
		}
	}
}