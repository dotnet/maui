using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Diagnostics;

internal class DiagnosticsManager : IDiagnosticsManager
{
	readonly IDiagnosticTagger[] _taggers;
	readonly IDiagnosticMetrics[] _metrics;
	readonly Dictionary<Type, IDiagnosticMetrics> _initializedMetrics = new();

	public DiagnosticsManager(IEnumerable<IDiagnosticMetrics> metrics, IEnumerable<IDiagnosticTagger> taggers, IMeterFactory? meterFactory = null)
	{
		_taggers = [.. taggers];
		_metrics = [.. metrics];

		ActivitySource = new ActivitySource(DiagnosticsIdentity.Namespace, DiagnosticsIdentity.Version);

		Meter = meterFactory?.Create(DiagnosticsIdentity.Namespace, DiagnosticsIdentity.Version);

		if (Meter is not null)
		{
			foreach (var metric in _metrics)
			{
				metric.Create(Meter);
				_initializedMetrics[metric.GetType()] = metric;
			}
		}
	}

	public ActivitySource ActivitySource { get; }

	public Meter? Meter { get; }

	public void GetTags(object source, out TagList tagList)
	{
		// Note: TagList is a struct and must always be passed around by reference
		// to avoid modifying a copy instead of the original.
		tagList = new TagList();

		foreach (var tagger in _taggers)
		{
			tagger.AddTags(source, ref tagList);
		}
	}

	public T? GetMetrics<T>()
		where T : IDiagnosticMetrics
	{
		if (!_initializedMetrics.TryGetValue(typeof(T), out var metrics))
		{
			return default;
		}

		return (T)metrics;
	}
}
