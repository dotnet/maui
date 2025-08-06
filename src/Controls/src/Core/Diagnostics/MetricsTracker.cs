using System;
using System.Diagnostics;
using Microsoft.Maui.Diagnostics;

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

        if (_activity is null)
            return;

        if (view is Element element)
        {
            _activity.SetTag("element.id", element.Id);
            _activity.SetTag("element.automation_id", element.AutomationId);
            _activity.SetTag("element.class_id", element.ClassId);
            _activity.SetTag("element.style_id", element.StyleId);
        }

        if (view is VisualElement ve)
        {
            _activity.SetTag("element.class", ve.@class);
            _activity.SetTag("element.frame", ve.Frame);
        }
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