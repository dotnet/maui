using UIKit;

namespace Microsoft.Maui.Platform
{
	public static partial class SemanticExtensions
	{
		public static void UpdateSemantics(this UIView platformView, IView view)
		{
			var semantics = view.Semantics;

			if (semantics == null)
				return;

			platformView.AccessibilityLabel = semantics.Description;
			platformView.AccessibilityHint = semantics.Hint;

			// UIControl elements automatically have IsAccessibilityElement set to true
			if (platformView is not UIControl && (!string.IsNullOrWhiteSpace(semantics.Hint) || !string.IsNullOrWhiteSpace(semantics.Description)))
				platformView.IsAccessibilityElement = true;

			if (semantics.IsHeading)
				platformView.AccessibilityTraits |= UIAccessibilityTrait.Header;
			else
				platformView.AccessibilityTraits &= ~UIAccessibilityTrait.Header;
		}
	}
}