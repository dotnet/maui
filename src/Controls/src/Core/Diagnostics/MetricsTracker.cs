using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Diagnostics;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Diagnostics;

enum DiagnosticsMeasuring
{
	None,
	Measure,
	Arrange
}

readonly struct MetricsTracker : IDisposable
{
	readonly Activity? _activity;
	readonly IView _view;
	readonly DiagnosticsMeasuring _activityName;

	public static MetricsTracker? Create(IView view, DiagnosticsMeasuring diagnosticsMeasuring)
	{
		if (RuntimeFeature.IsMeterSupported)
			return new MetricsTracker(view, diagnosticsMeasuring);

		return null;
	}

	MetricsTracker(IView view, DiagnosticsMeasuring diagnosticsMeasuring)
	{
		_view = view;
		_activity = view.StartActivity($"{diagnosticsMeasuring}");
		_activityName = diagnosticsMeasuring;
	}

	public void Dispose()
	{
		_activity?.Stop();

		switch (_activityName)
		{
			case DiagnosticsMeasuring.Arrange:
				_view.RecordArrange(_activity?.Duration);
				break;
			case DiagnosticsMeasuring.Measure:
				_view.RecordMeasure(_activity?.Duration);
				break;
			default:
				break;
		}

		_activity?.Dispose();
	}
}