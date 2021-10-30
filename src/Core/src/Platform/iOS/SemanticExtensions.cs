using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public static partial class SemanticExtensions
	{
		public static void SetSemanticFocus(this IView element)
		{
			if (element?.Handler?.NativeView == null)
				throw new NullReferenceException("Can't access view from a null handler");

			if (element.Handler.NativeView is not NSObject nativeView)
				return;

			UIAccessibility.PostNotification(UIAccessibilityPostNotification.LayoutChanged, nativeView);
		}

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
