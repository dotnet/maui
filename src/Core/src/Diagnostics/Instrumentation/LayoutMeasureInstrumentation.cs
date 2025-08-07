using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

readonly struct LayoutMeasureInstrumentation(IView view) : IDiagnosticInstrumentation
{
	readonly Activity? _activity = view.StartActivity("Measure");

	public void Dispose() =>
		view.StopDiagnostics(_activity, this);

	public void Stopped(MauiDiagnostics diag, in TagList tagList) =>
		diag.GetMetrics<LayoutDiagnosticMetrics>()?.RecordMeasure(_activity?.Duration, in tagList);
}
