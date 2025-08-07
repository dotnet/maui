namespace Microsoft.Maui.Diagnostics;

internal static class DiagnosticInstrumentation
{
	public static LayoutMeasureInstrumentation? StartLayoutMeasure(IView view) =>
		RuntimeFeature.IsMeterSupported
			? new LayoutMeasureInstrumentation(view)
			: null;

	public static LayoutArrangeInstrumentation? StartLayoutArrange(IView view) =>
		RuntimeFeature.IsMeterSupported
			? new LayoutArrangeInstrumentation(view)
			: null;
}
