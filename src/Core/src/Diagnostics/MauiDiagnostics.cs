using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Diagnostics;

internal class MauiDiagnostics
{
	const string DiagnosticsNamespace = "Microsoft.Maui";
	const string DiagnosticsVersion = "1.0.0";

	readonly IDiagnosticTagger[] _taggers;
	readonly IDiagnosticMetrics[] _metrics;
	readonly Dictionary<Type, IDiagnosticMetrics> _initializedMetrics = new();

	public MauiDiagnostics(IEnumerable<IDiagnosticMetrics> metrics, IEnumerable<IDiagnosticTagger> taggers, IMeterFactory? meterFactory = null)
	{
		_taggers = [.. taggers];
		_metrics = [.. metrics];

		ActivitySource = new ActivitySource(DiagnosticsNamespace, DiagnosticsVersion);

		Meter = meterFactory?.Create(DiagnosticsNamespace, DiagnosticsVersion);

		if (Meter is not null)
		{
			foreach (var metric in _metrics)
			{
				metric.Create(Meter);
				_initializedMetrics[metric.GetType()] = metric;
			}
		}
	}

	internal ActivitySource ActivitySource { get; }

	internal Meter? Meter { get; }

	internal void GetTags(object source, out TagList tagList)
	{
		tagList = new TagList();

		foreach (var tagger in _taggers)
		{
			tagger.AddTags(source, ref tagList);
		}
	}

	internal T? GetMetrics<T>()
		where T : IDiagnosticMetrics
	{
		if (!_initializedMetrics.TryGetValue(typeof(T), out var metrics))
		{
			return default;
		}

		return (T)metrics;
	}
}
