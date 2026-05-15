using System.Text;
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

		/// <summary>
		/// Posts a VoiceOver screen changed notification to return focus to the specified view.
		/// This is typically used when an input view is dismissed and focus should return to the original control.
		/// </summary>
		/// <param name="platformView">The platform view that should receive focus</param>
		internal static void PostAccessibilityFocusNotification(this UIView platformView)
		{
			// TODO: Make public for .NET 11.
			UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, platformView);
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

			if (inputView is null)
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

		public static void UpdateSemantics(this UIView platformView, IView view)
		{
			var semantics = view?.Semantics;

			if (semantics is null)
			{
				return;
			}

			// For layout containers with Hint or Description set, synthesize an AccessibilityLabel
			// from the MAUI virtual children's text so VoiceOver reads both the children's content
			// AND the container's hint in a single announcement — matching Android TalkBack behavior.
			// We read from the virtual view tree (not platformView.Subviews) to avoid timing issues:
			// UpdateSemantics can be called before platform children are attached.
			if (view is ILayout layout &&
				(!string.IsNullOrWhiteSpace(semantics.Hint) || !string.IsNullOrWhiteSpace(semantics.Description)))
			{
				var synthesizedLabel = SynthesizeAccessibilityLabelFromChildren(layout);

				if (!string.IsNullOrWhiteSpace(synthesizedLabel))
				{
					// If there's an explicit Description, prepend it to the children's text
					if (!string.IsNullOrWhiteSpace(semantics.Description))
					{
						platformView.AccessibilityLabel = $"{semantics.Description}, {synthesizedLabel}";
					}
					else
					{
						platformView.AccessibilityLabel = synthesizedLabel;
					}
				}
				else
				{
					// No children text found — fall back to Description only
					platformView.AccessibilityLabel = semantics.Description;
				}

				platformView.AccessibilityHint = semantics.Hint;

				// Make the container the primary accessibility element so VoiceOver announces
				// "[children text], [hint]" as a single focus unit.
				platformView.IsAccessibilityElement = true;

				UpdateSemanticsHeading(platformView, semantics);
				return;
			}

			UpdateSemantics(platformView, semantics);
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

			UpdateSemanticsHeading(platformView, semantics);
		}

		static void UpdateSemanticsHeading(UIView platformView, Semantics semantics)
		{
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

		/// <summary>
		/// Synthesizes an accessibility label by collecting text from all IText children in the layout.
		/// Uses the MAUI virtual view tree (not platform subviews) to avoid timing issues.
		/// </summary>
		static string? SynthesizeAccessibilityLabelFromChildren(ILayout layout)
		{
			var sb = new StringBuilder();
			CollectChildrenText(layout, sb);
			return sb.Length > 0 ? sb.ToString() : null;
		}

		static void CollectChildrenText(ILayout layout, StringBuilder sb)
		{
			for (int i = 0; i < layout.Count; i++)
			{
				var child = layout[i];

				// Prefer explicit SemanticProperties.Description over raw text
				if (child.Semantics?.Description is string childDesc && !string.IsNullOrWhiteSpace(childDesc))
				{
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}

					sb.Append(childDesc);
				}
				else if (child is IText textElement && !string.IsNullOrWhiteSpace(textElement.Text))
				{
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}

					sb.Append(textElement.Text);
				}
				else if (child is ILayout childLayout)
				{
					// Recurse into nested layouts to collect text from their children
					CollectChildrenText(childLayout, sb);
				}
			}
		}
	}
}