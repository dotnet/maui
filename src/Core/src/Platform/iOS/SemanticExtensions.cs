using UIKit;

namespace Microsoft.Maui.Platform
{
	public static partial class SemanticExtensions
	{
		public static void UpdateSemantics(this UIView nativeView, IView view)
		{
			var semantics = view.Semantics;

			if (semantics == null)
				return;

			nativeView.AccessibilityLabel = semantics.Description;
			nativeView.AccessibilityHint = semantics.Hint;

			// UIControl elements automatically have IsAccessibilityElement set to true
			if (nativeView is not UIControl && (!string.IsNullOrWhiteSpace(semantics.Hint) || !string.IsNullOrWhiteSpace(semantics.Description)))
				nativeView.IsAccessibilityElement = true;

			if (semantics.IsHeading)
				nativeView.AccessibilityTraits |= UIAccessibilityTrait.Header;
			else
				nativeView.AccessibilityTraits &= ~UIAccessibilityTrait.Header;
		}
	}
}