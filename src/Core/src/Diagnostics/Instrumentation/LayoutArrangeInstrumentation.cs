using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Instrumentation for the layout arrange phase of a view.
/// </summary>
readonly struct LayoutArrangeInstrumentation(IView view) : IDiagnosticInstrumentation
{
	readonly Activity? _activity = view.StartDiagnosticActivity("Arrange");

	/// <summary>
	/// Disposes the instrumentation and stops the diagnostic activity.
	/// </summary>
	public void Dispose() =>
		view.StopDiagnostics(_activity, this);

	/// <summary>
	/// Records the stopping of the instrumentation and publishes various metrics.
	/// </summary>
	/// <param name="diagnostics">The <see cref="IDiagnosticsManager"/> instance.</param>
	/// <param name="tagList">The tags associated with the instrumentation.</param>
	public void Stopped(IDiagnosticsManager diagnostics, in TagList tagList) =>
		diagnostics.GetMetrics<LayoutDiagnosticMetrics>()?.RecordArrange(_activity?.Duration, in tagList);
}
