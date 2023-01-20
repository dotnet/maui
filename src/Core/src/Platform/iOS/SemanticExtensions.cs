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

			if ((!string.IsNullOrWhiteSpace(semantics.Hint) || !string.IsNullOrWhiteSpace(semantics.Description)))
			{
				// Most UIControl elements automatically have IsAccessibilityElement set to true
				if (platformView is not UIControl)
					platformView.IsAccessibilityElement = true;
				// UIStepper and UIPageControl inherit from UIControl but iOS marks `IsAccessibilityElement` to false
				// because they are composite controls.
				else if (platformView is UIStepper || platformView is UIPageControl)
					platformView.IsAccessibilityElement = true;
			}

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