using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Instrumentation for measuring layout operations in a view.
/// </summary>
readonly struct LayoutMeasureInstrumentation : System.IDisposable
{
	readonly IView _view;
	readonly IDiagnosticsManager _diagnostics;
	readonly LayoutDiagnosticMetrics? _metrics;
	readonly Activity? _activity;
	readonly long _metricsStartTimestamp;

	public LayoutMeasureInstrumentation(IView view, IDiagnosticsManager diagnostics, LayoutDiagnosticMetrics? metrics)
	{
		_view = view;
		_diagnostics = diagnostics;
		_metrics = metrics;

		if (diagnostics.HasActivityListeners)
		{
			diagnostics.GetTags(view, out var tagList);
			_activity = diagnostics.ActivitySource.StartActivity(
				ActivityKind.Internal,
				name: $"Measure {view.GetType().Name}",
				tags: tagList);
		}
		else
		{
			_activity = null;
		}

		_metricsStartTimestamp = metrics?.IsMeasureDurationEnabled == true
			? Stopwatch.GetTimestamp()
			: 0;
	}

	/// <summary>
	/// Disposes the instrumentation and stops the diagnostic activity.
	/// </summary>
	public void Dispose()
	{
		var metrics = _metrics;
		var recordDuration = _metricsStartTimestamp != 0 && metrics?.IsMeasureDurationEnabled == true;
		var duration = recordDuration
			? LayoutDiagnosticMetrics.GetElapsedNanoseconds(_metricsStartTimestamp)
			: 0;

		_activity?.Stop();

		if (metrics?.IsMeasureEnabled == true)
		{
			_diagnostics.GetTags(_view, out var tagList);
			metrics.RecordMeasure(duration, recordDuration, in tagList);
		}

		_activity?.Dispose();
	}
}
