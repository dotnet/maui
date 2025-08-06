using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;

namespace Microsoft.Maui.Diagnostics;

internal class MauiDiagnostics
{
	const string DiagnosticsNamespace = "Microsoft.Maui.Diagnostics";
	const string DiagnosticsVersion = "1.0.0";

	readonly IDiagnosticTagger[] _taggers;

	public MauiDiagnostics(IEnumerable<IDiagnosticTagger> taggers, IMeterFactory? meterFactory = null)
	{
		_taggers = taggers.ToArray();

		ActivitySource = new ActivitySource(DiagnosticsNamespace, DiagnosticsVersion);

		Meters = meterFactory?.Create(DiagnosticsNamespace, DiagnosticsVersion);

		MeasureCounter = Meters?.CreateCounter<int>("maui.layout.measure_count", "{times}", "The number of times a measure happened.");
		ArrangeCounter = Meters?.CreateCounter<int>("maui.layout.arrange_count", "{times}", "The number of times an arrange happened.");

		MeasureHistogram = Meters?.CreateHistogram<int>("maui.layout.measure_duration", "ns");
		ArrangeHistogram = Meters?.CreateHistogram<int>("maui.layout.arrange_duration", "ns");
	}


	internal ActivitySource ActivitySource { get; }

	internal Meter? Meters { get; }

	internal Counter<int>? MeasureCounter { get; }
	internal Counter<int>? ArrangeCounter { get; }
	internal Histogram<int>? MeasureHistogram { get; }
	internal Histogram<int>? ArrangeHistogram { get; }

	internal void AddTags(object source, ref TagList tagList)
	{
		foreach (var tagger in _taggers)
		{
			tagger.AddTags(source, ref tagList);
		}
	}
}
