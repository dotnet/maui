namespace Microsoft.Maui.Diagnostics;

internal static class DiagnosticInstrumentation
{
	public static MeasureInstrumentation? StartMeasure(IView view) =>
		RuntimeFeature.IsMeterSupported
			? new MeasureInstrumentation(view)
			: null;

	public static ArrangeInstrumentation? StartArrange(IView view) =>
		RuntimeFeature.IsMeterSupported
			? new ArrangeInstrumentation(view)
			: null;
}
