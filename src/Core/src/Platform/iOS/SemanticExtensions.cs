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
		
		/// <summary>
		/// Posts a VoiceOver screen changed notification to return focus to the specified view.
		/// This is typically used when an input view is dismissed and focus should return to the original control.
		/// </summary>
		/// <param name="platformView">The platform view that should receive focus</param>
		internal static void PostAccessibilityFocusNotification(this UIView platformView)
		{
			// TODO: Make public for .NET 11.
			
			if (UIAccessibility.IsVoiceOverRunning)
			{
				UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, platformView);
			}
		}
		
		/// <summary>
		/// Posts a VoiceOver screen changed notification for an input view when it becomes visible.
		/// This ensures VoiceOver shifts focus to the input view (e.g., UIPickerView, UIDatePicker) when it appears.
		/// </summary>
		/// <param name="platformView">The platform view that hosts the input view</param>
		/// <param name="inputView">The input view that should receive focus</param>
		internal static void PostAccessibilityFocusNotification(this UIView platformView, UIView? inputView)
		{
			// TODO: Make public for .NET 11.

			if (inputView is null || !UIAccessibility.IsVoiceOverRunning)
			{
				return;
			}

			// By the time EditingDidBegin is called, the InputView should be ready
			// If InputView is not in window yet, post with minimal delay
			if (inputView.Window is not null)
			{
				UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, inputView);
			}
			else
			{
				// Use DispatchQueue with minimal delay if not in window yet
				Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(() =>
				{
					if (inputView.Window is not null)
					{
						UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, inputView);
					}
				});
			}
		}
		
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