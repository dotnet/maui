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

			var desc = semantics.Description;
			var hint = semantics.Hint;

			if (platformView is UISearchBar searchBar)
			{
				var textField = searchBar.GetSearchTextField();

				if (textField == null)
					return;
				else
					platformView = textField;

				if (!string.IsNullOrWhiteSpace(desc))
				{
					textField.AccessibilityLabel = desc;
				}

				if (!string.IsNullOrWhiteSpace(hint))
				{
					textField.AccessibilityHint = hint;
				}
			}
			else
			{
				platformView.AccessibilityLabel = desc;
				platformView.AccessibilityHint = hint;
			}

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