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

			// Null semantics (e.g. ClearValue): reverse any previous promotion so the layout
			// stops announcing stale content and its children become navigable again.
			if (semantics is null)
			{
				if (view is ILayout && platformView is not UIControl && platformView.IsAccessibilityElement)
				{
					platformView.IsAccessibilityElement = false;
					platformView.AccessibilityLabel = null;
					platformView.AccessibilityHint = null;
					if (platformView is MauiView clearedMauiView)
					{
						clearedMauiView.SynthesizeAccessibilityLabelFromChildren = false;
					}
				}

				return;
			}

			// For layout containers with a Hint set, decide on an AccessibilityLabel that gives
			// VoiceOver the children's content (matching Android TalkBack behavior).
			// - If the developer explicitly set Description, respect it as the label — don't
			//   second-guess by appending children. This preserves the pre-fix contract for
			//   apps that deliberately set Description as a curated summary.
			// - If only Hint is set (no Description), synthesize a label from the children's
			//   text so VoiceOver reads "[children], [hint]" instead of just "[hint]".
			// We gate on Hint only (not Description) so Description-only layouts retain their
			// existing legacy-path behavior.
			if (view is ILayout layout &&
				!string.IsNullOrWhiteSpace(semantics.Hint))
			{
				if (!string.IsNullOrWhiteSpace(semantics.Description))
				{
					// Explicit Description wins — honor the developer's curated label.
					platformView.AccessibilityLabel = semantics.Description;
					if (platformView is MauiView mauiViewWithDesc)
					{
						mauiViewWithDesc.SynthesizeAccessibilityLabelFromChildren = false;
					}
				}
				else if (platformView is MauiView mauiViewLayout)
				{
					// Defer synthesis to MauiView.AccessibilityLabel's getter so child text changes
					// are picked up on each VoiceOver focus (avoids stale one-shot snapshot).
					mauiViewLayout.SynthesizeAccessibilityLabelFromChildren = true;
					mauiViewLayout.AccessibilityLabel = null;
				}
				else
				{
					// Non-MauiView platform view (rare for ILayout): fall back to one-shot snapshot.
					var synthesizedLabel = SynthesizeAccessibilityLabelFromChildren(layout);
					platformView.AccessibilityLabel = !string.IsNullOrWhiteSpace(synthesizedLabel)
						? synthesizedLabel
						: null;
				}

				platformView.AccessibilityHint = semantics.Hint;

				// Make the container the primary accessibility element so VoiceOver announces
				// "[label], [hint]" as a single focus unit.
				platformView.IsAccessibilityElement = true;

				// If a TapGestureRecognizer was wired up first, GesturePlatformManager may have
				// already set ShouldGroupAccessibilityChildren=true. Clear it now: once the layout
				// is a leaf accessibility element the grouping flag is redundant, and some iOS
				// versions silently drop the parent's accessibilityLabel/hint when both are set.
				platformView.ShouldGroupAccessibilityChildren = false;

				// Safe to early-return: an ILayout MAUI view never produces a UISearchBar/UIControl/
				// UIStepper/UIPageControl platform view, so the internal UpdateSemantics branches for
				// those types are not applicable here. Heading trait is explicitly applied below.
				UpdateSemanticsHeading(platformView, semantics);
				return;
			}

			// If a layout previously had Hint set (triggering the synthesis branch above which
			// promotes the layout to an accessibility element) and the Hint/Description was later
			// cleared, restore the layout to a non-leaf so VoiceOver can navigate into its children
			// again. Layouts default to IsAccessibilityElement=false; the legacy path below only
			// flips it to true and never back to false, so without this reset the layout would
			// remain a silent leaf.
			if (view is ILayout
				&& platformView is not UIControl
				&& string.IsNullOrWhiteSpace(semantics.Hint)
				&& string.IsNullOrWhiteSpace(semantics.Description)
				&& platformView.IsAccessibilityElement)
			{
				platformView.IsAccessibilityElement = false;
				if (platformView is MauiView demotedMauiView)
				{
					demotedMauiView.SynthesizeAccessibilityLabelFromChildren = false;
				}
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
		/// Maximum recursion depth for <see cref="CollectChildrenText"/>. Caps traversal of deeply
		/// nested layouts so pathological hierarchies cannot stall accessibility updates.
		/// </summary>
		const int MaxChildTextRecursionDepth = 10;

		/// <summary>
		/// Synthesizes an accessibility label by collecting text from all IText children in the layout.
		/// Uses the MAUI virtual view tree (not platform subviews) to avoid timing issues.
		/// </summary>
		internal static string? SynthesizeAccessibilityLabelFromChildren(ILayout layout)
		{
			var sb = new StringBuilder();
			CollectChildrenText(layout, sb, depth: 0);
			return sb.Length > 0 ? sb.ToString() : null;
		}

		static void CollectChildrenText(ILayout layout, StringBuilder sb, int depth)
		{
			if (depth >= MaxChildTextRecursionDepth)
			{
				return;
			}

			for (int i = 0; i < layout.Count; i++)
			{
				var child = layout[i];

				// Skip non-visible children: once the parent is a leaf accessibility element,
				// the platform only sees the synthesized string, so hidden text must be filtered here.
				if (child.Visibility != Visibility.Visible)
				{
					continue;
				}

				// Prefer explicit SemanticProperties.Description over raw text
				if (child.Semantics?.Description is string childDesc && !string.IsNullOrWhiteSpace(childDesc))
				{
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}

					sb.Append(childDesc);
				}
				else if (child is IText textElement
					&& child is not ITextInput
					&& !string.IsNullOrWhiteSpace(textElement.Text))
				{
					// Skip ITextInput (Entry/Editor/SearchBar): their .Text is user input which
					// would leak into the layout's accessibility label and never refresh as the
					// user types. Entries/editors are independently focusable anyway.
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}

					sb.Append(textElement.Text);
				}
				else if (child is ILayout childLayout)
				{
					// Recurse into nested layouts to collect text from their children
					CollectChildrenText(childLayout, sb, depth + 1);
				}
			}
		}
	}
}