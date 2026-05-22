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
	public static LayoutMeasureInstrumentation? StartLayoutMeasure(IView view)
	{
		if (!RuntimeFeature.IsMeterSupported)
		{
			return null;
		}

		var diagnostics = view.GetMauiDiagnostics();
		if (diagnostics is null)
		{
			return null;
		}

		var metrics = diagnostics.GetMetrics<LayoutDiagnosticMetrics>();
		if (!diagnostics.HasActivityListeners && metrics?.IsMeasureEnabled != true)
		{
			return null;
		}

		return new LayoutMeasureInstrumentation(view, diagnostics, metrics);
	}

	/// <summary>
	/// Starts layout arrange instrumentation for the specified view.
	/// </summary>
	/// <param name="view">The view to instrument.</param>
	/// <returns>Returns an instance of <see cref="LayoutArrangeInstrumentation"/> if instrumentation is supported; otherwise, null.</returns>
	public static LayoutArrangeInstrumentation? StartLayoutArrange(IView view)
	{
		if (!RuntimeFeature.IsMeterSupported)
		{
			return null;
		}

		var diagnostics = view.GetMauiDiagnostics();
		if (diagnostics is null)
		{
			return null;
		}

		var metrics = diagnostics.GetMetrics<LayoutDiagnosticMetrics>();
		if (!diagnostics.HasActivityListeners && metrics?.IsArrangeEnabled != true)
		{
			return null;
		}

		return new LayoutArrangeInstrumentation(view, diagnostics, metrics);
	}
}
