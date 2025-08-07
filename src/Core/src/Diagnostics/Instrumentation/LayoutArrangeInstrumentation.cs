using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

readonly struct LayoutArrangeInstrumentation(IView view) : IDiagnosticInstrumentation
{
	readonly Activity? _activity = view.StartActivity("Arrange");

	public void Dispose() =>
		view.StopDiagnostics(_activity, this);

	public void Stopped(MauiDiagnostics diag, in TagList tagList) =>
		diag.GetMetrics<LayoutDiagnosticMetrics>()?.RecordArrange(_activity?.Duration, in tagList);
}
