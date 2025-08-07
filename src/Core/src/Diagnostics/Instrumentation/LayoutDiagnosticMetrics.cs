using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Diagnostics;

internal class LayoutDiagnosticMetrics : IDiagnosticMetrics
{
	public Counter<int>? MeasureCounter { get; private set; }

	public Histogram<int>? MeasureHistogram { get; private set; }

	internal Counter<int>? ArrangeCounter { get; private set; }

	internal Histogram<int>? ArrangeHistogram { get; private set; }

	public void Create(Meter meter)
	{
		MeasureCounter = meter.CreateCounter<int>("maui.layout.measure_count", "{times}", "The number of times a measure happened.");
		MeasureHistogram = meter.CreateHistogram<int>("maui.layout.measure_duration", "ns");

		ArrangeCounter = meter.CreateCounter<int>("maui.layout.arrange_count", "{times}", "The number of times an arrange happened.");
		ArrangeHistogram = meter.CreateHistogram<int>("maui.layout.arrange_duration", "ns");
	}

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
