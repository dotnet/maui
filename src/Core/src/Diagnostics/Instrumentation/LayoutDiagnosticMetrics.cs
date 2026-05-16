using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Defines diagnostic metrics for layout operations, including measure and arrange durations.
/// </summary>
internal class LayoutDiagnosticMetrics : IDiagnosticMetrics
{
	/// <summary>
	/// Gets the counter metric for the number of measure operations performed.
	/// </summary>
	public Counter<int>? MeasureCounter { get; private set; }

	/// <summary>
	/// Gets the histogram metric for the duration of measure operations in nanoseconds.
	/// </summary>
	public Histogram<int>? MeasureHistogram { get; private set; }

	/// <summary>
	/// Gets the counter metric for the number of arrange operations performed.
	/// </summary>
	internal Counter<int>? ArrangeCounter { get; private set; }

	/// <summary>
	/// Gets the histogram metric for the duration of arrange operations in nanoseconds.
	/// </summary>
	internal Histogram<int>? ArrangeHistogram { get; private set; }

	internal bool IsMeasureEnabled =>
		MeasureCounter?.Enabled == true ||
		MeasureHistogram?.Enabled == true;

	internal bool IsMeasureDurationEnabled => MeasureHistogram?.Enabled == true;

	internal bool IsArrangeEnabled =>
		ArrangeCounter?.Enabled == true ||
		ArrangeHistogram?.Enabled == true;

	internal bool IsArrangeDurationEnabled => ArrangeHistogram?.Enabled == true;

	/// <inheritdoc/>
	public void Create(Meter meter)
	{
		MeasureCounter = meter.CreateCounter<int>("maui.layout.measure_count", "{times}", "The number of times a measure happened.");
		MeasureHistogram = meter.CreateHistogram<int>("maui.layout.measure_duration", "ns");

		ArrangeCounter = meter.CreateCounter<int>("maui.layout.arrange_count", "{times}", "The number of times an arrange happened.");
		ArrangeHistogram = meter.CreateHistogram<int>("maui.layout.arrange_duration", "ns");
	}

	/// <summary>
	/// Records a measure operation with an optional duration and associated tags.
	/// </summary>
	/// <param name="duration">The duration of the measure operation in nanoseconds.</param>
	/// <param name="recordDuration">Whether a duration should be recorded.</param>
	/// <param name="tagList">The tags associated with the measure operation.</param>
	public void RecordMeasure(int duration, bool recordDuration, in TagList tagList)
	{
		if (MeasureCounter?.Enabled == true)
		{
			MeasureCounter.Add(1, tagList);
		}

		if (recordDuration && MeasureHistogram?.Enabled == true)
		{
			MeasureHistogram.Record(duration, tagList);
		}
	}

	/// <summary>
	/// Records an arrange operation with an optional duration and associated tags.
	/// </summary>
	/// <param name="duration">The duration of the arrange operation in nanoseconds.</param>
	/// <param name="recordDuration">Whether a duration should be recorded.</param>
	/// <param name="tagList">The tags associated with the arrange operation.</param>
	public void RecordArrange(int duration, bool recordDuration, in TagList tagList)
	{
		if (ArrangeCounter?.Enabled == true)
		{
			ArrangeCounter.Add(1, tagList);
		}

		if (recordDuration && ArrangeHistogram?.Enabled == true)
		{
			ArrangeHistogram.Record(duration, tagList);
		}
	}

	internal static int GetElapsedNanoseconds(long startTimestamp)
	{
		var elapsedTimestamp = Stopwatch.GetTimestamp() - startTimestamp;
		return (int)(elapsedTimestamp * (1_000_000_000.0 / Stopwatch.Frequency));
	}
}
