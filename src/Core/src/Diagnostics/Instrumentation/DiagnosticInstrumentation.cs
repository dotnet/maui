namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Provides methods to start various instrumentation operations.
/// </summary>
internal static class DiagnosticInstrumentation
{
	/// <summary>
	/// Starts layout measure instrumentation for the specified view.
	/// </summary>
	/// <param name="view">The view to instrument.</param>
	/// <returns>Returns an instance of <see cref="LayoutMeasureInstrumentation"/> if instrumentation is supported; otherwise, null.</returns>
	public static LayoutMeasureInstrumentation? StartLayoutMeasure(IView view) =>
		RuntimeFeature.IsMeterSupported
			? new LayoutMeasureInstrumentation(view)
			: null;

	/// <summary>
	/// Starts layout arrange instrumentation for the specified view.
	/// </summary>
	/// <param name="view">The view to instrument.</param>
	/// <returns>Returns an instance of <see cref="LayoutArrangeInstrumentation"/> if instrumentation is supported; otherwise, null.</returns>
	public static LayoutArrangeInstrumentation? StartLayoutArrange(IView view) =>
		RuntimeFeature.IsMeterSupported
			? new LayoutArrangeInstrumentation(view)
			: null;
}
