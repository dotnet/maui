using UIKit;

namespace Microsoft.Maui.Platform
{
	public static partial class SemanticExtensions
	{
		internal static void UpdateSemantics(this UIBarItem platformView, Semantics? semantics)
		{
			if (semantics == null)
				return;

			platformView.AccessibilityLabel = semantics.Description;
			platformView.AccessibilityHint = semantics.Hint;

			var accessibilityTraits = platformView.AccessibilityTraits;
			var hasHeader = (accessibilityTraits & UIAccessibilityTrait.Header) == UIAccessibilityTrait.Header;

			if (semantics.IsHeading)
			{
				if (!hasHeader)
				{
					platformView.AccessibilityTraits = accessibilityTraits | UIAccessibilityTrait.Header;
				}
			}
			else
			{
				if (hasHeader)
				{
					platformView.AccessibilityTraits = accessibilityTraits & ~UIAccessibilityTrait.Header;
				}
			}
		}

		public static void UpdateSemantics(this UIView platformView, IView view) =>
			UpdateSemantics(platformView, view?.Semantics);

		internal static void UpdateSemantics(this UIView platformView, Semantics? semantics)
		{
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

			if (!string.IsNullOrWhiteSpace(semantics.Hint) || !string.IsNullOrWhiteSpace(semantics.Description))
			{
				// Most UIControl elements automatically have IsAccessibilityElement set to true
				if (platformView is not UIControl)
					platformView.IsAccessibilityElement = true;
				// UIStepper and UIPageControl inherit from UIControl but iOS marks `IsAccessibilityElement` to false
				// because they are composite controls.
				else if (platformView is UIStepper || platformView is UIPageControl)
					platformView.IsAccessibilityElement = true;
			}

			var accessibilityTraits = platformView.AccessibilityTraits;
			var hasHeader = (accessibilityTraits & UIAccessibilityTrait.Header) == UIAccessibilityTrait.Header;

			if (semantics.IsHeading)
			{
				if (!hasHeader)
				{
					platformView.AccessibilityTraits = accessibilityTraits | UIAccessibilityTrait.Header;
				}
			}
			else
			{
				if (hasHeader)
				{
					platformView.AccessibilityTraits = accessibilityTraits & ~UIAccessibilityTrait.Header;
				}
			}
		}
	}
}