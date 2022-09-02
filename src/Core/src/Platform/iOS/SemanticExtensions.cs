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

			if (platformView is UISearchBar searchBar)
			{
				var textField = searchBar.GetSearchTextField();

				if (textField == null)
					return;

				platformView = textField;
			}

			platformView.AccessibilityLabel = semantics.Description;
			platformView.AccessibilityHint = semantics.Hint;

			// UIControl elements automatically have IsAccessibilityElement set to true
			if (platformView is not UIControl && (!string.IsNullOrWhiteSpace(semantics.Hint) || !string.IsNullOrWhiteSpace(semantics.Description)))
				platformView.IsAccessibilityElement = true;

			if (semantics.IsHeading)
			{
				if ((platformView.AccessibilityTraits & UIAccessibilityTrait.Header) != UIAccessibilityTrait.Header)
					platformView.AccessibilityTraits |= UIAccessibilityTrait.Header;
			}
			else
			{
				if ((platformView.AccessibilityTraits & UIAccessibilityTrait.Header) == UIAccessibilityTrait.Header)
					platformView.AccessibilityTraits &= ~UIAccessibilityTrait.Header;
			}
		}
	}
}