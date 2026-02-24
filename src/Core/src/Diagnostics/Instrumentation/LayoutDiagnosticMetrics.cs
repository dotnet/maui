using System;
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
	/// <param name="duration">The duration of the measure operation.</param>
	/// <param name="tagList">The tags associated with the measure operation.</param>
	public void RecordMeasure(TimeSpan? duration, in TagList tagList)
	{
		MeasureCounter?.Add(1, tagList);

		if (duration is not null)
		{
#if NET9_0_OR_GREATER
			MeasureHistogram?.Record((int)duration.Value.TotalNanoseconds, tagList);
#else
			MeasureHistogram?.Record((int)(duration.Value.TotalMilliseconds * 1_000_000), tagList);
#endif
		}
	}

	/// <summary>
	/// Records an arrange operation with an optional duration and associated tags.
	/// </summary>
	/// <param name="duration">The duration of the arrange operation.</param>
	/// <param name="tagList">The tags associated with the arrange operation.</param>
	public void RecordArrange(TimeSpan? duration, in TagList tagList)
	{
		ArrangeCounter?.Add(1, tagList);

		if (duration is not null)
		{
#if NET9_0_OR_GREATER
			ArrangeHistogram?.Record((int)duration.Value.TotalNanoseconds, tagList);
#else
			ArrangeHistogram?.Record((int)(duration.Value.TotalMilliseconds * 1_000_000), tagList);
#endif
		}
	}
}
