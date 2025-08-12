namespace Microsoft.Maui.Platform;

/// <summary>
/// Provides platform-specific measure invalidation control for iOS views.
/// </summary>
public interface IPlatformMeasureInvalidationController
{
	/// <summary>
	/// Schedules measure invalidation to occur when the view is moved to a window.
	/// This is used to handle scenarios where invalidation is attempted before the view is attached to a window.
	/// </summary>
	void InvalidateAncestorsMeasuresWhenMovedToWindow();

	/// <summary>
	/// Invalidates the current view via SetNeedsLayout and returns whether to continue propagating the invalidation to ancestors or not.
	/// </summary>
	/// <param name="isPropagating">True if this invalidation is being propagated from a descendant view, false if this is the initial view that triggered the invalidation.</param>
	/// <returns>True to continue propagating invalidation to ancestor views, false to stop propagation.</returns>
	bool InvalidateMeasure(bool isPropagating = false);
}