using UIKit;

namespace Microsoft.Maui.Platform;

internal interface IPlatformMeasureInvalidationController
{
	/// <summary>
	/// Marks a UIView with this tag when it's not possible to use inheritance to override SetNeedsLayout.
	/// When this tag is found, the invalidation is automatically propagated to the parent.
	/// </summary>
	const nint PropagationProxyTag = 0x845fff;

	static int _invalidationDepth;

	/// <summary>
	/// Gets whether we're currently running an invalidation pass.
	/// </summary>
	public static bool IsInvalidating => _invalidationDepth > 0;

	/// <summary>
	/// Gets whether the current invalidation pass is invalidating the view which triggered the invalidation.
	/// </summary>
	public static bool IsInvalidatingSelf => _invalidationDepth == 1;

	/// <summary>
	/// Gets whether the current invalidation pass is invalidating the ancestors of the view which triggered the invalidation.
	/// </summary>
	public static bool IsPropagatingInvalidation => _invalidationDepth > 1;

	public static void BeginInvalidation() => _invalidationDepth = 0;
	public static void BeginAncestorsInvalidation() => _invalidationDepth = 1;
	public static void EndInvalidation() => _invalidationDepth = 0;

	public static void SetNeedsLayout(UIView view)
	{
		++_invalidationDepth;
		view.SetNeedsLayout();
	}

	void InvalidateAncestorsMeasuresWhenMovedToWindow();
}