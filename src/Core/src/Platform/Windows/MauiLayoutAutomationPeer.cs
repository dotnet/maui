#nullable enable
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	// TODO: Make this class public in .NET11.0
	internal partial class MauiLayoutAutomationPeer : FrameworkElementAutomationPeer
	{
		internal MauiLayoutAutomationPeer(LayoutPanel owner) : base(owner) { }

		// Returns the cross-platform layout type name (e.g. "Grid", "VerticalStackLayout") so
		// accessibility tools can distinguish between different layout types. Falls back to "Panel"
		// when the cross-platform layout is unavailable.
		protected override string GetClassNameCore()
		{
			if (Owner is MauiPanel panel)
			{
				return panel.CrossPlatformLayout?.GetType().Name ?? nameof(Panel);
			}

			return nameof(Panel);
		}

		// Layouts are structural containers — AutomationControlType.Pane signals to screen readers
		// that this is a grouping/container element, not an interactive control.
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Pane;
		}

		// Layouts are never keyboard-focusable — they are structural containers, not interactive elements.
		protected override bool IsKeyboardFocusableCore() => false;

		// Layouts/panels are structural containers. By default we exclude them from the UIA
		// Control view so screen readers (Narrator, NVDA) don't stop on every nested Grid /
		// StackLayout / ContentView while a user is navigating a page. AutomationId alone is
		// overwhelmingly used as a UI testing hook (Appium / WinAppDriver look up elements by
		// id in the raw view, which is unaffected), not as an accessibility landmark.
		//
		// We DO opt in to the Control view when the developer signals that this panel is
		// meaningful to assistive tech:
		//   * AutomationProperties.IsInAccessibleTree="True" on the cross-platform view
		//     (mapped to AccessibilityView = Content / Control on the platform view), or
		//   * SemanticProperties.Description / Hint (mapped to AutomationProperties.Name /
		//     HelpText on the platform view) -- if the developer wrote a description, they
		//     want screen readers to announce it.
		protected override bool IsControlElementCore()
		{
			if (Owner is not Panel panel)
			{
				return false;
			}

			var accessibilityView = panel.ReadLocalValue(AutomationProperties.AccessibilityViewProperty);

			if (accessibilityView is AccessibilityView.Raw)
			{
				return false;
			}
			else if (accessibilityView is AccessibilityView.Control or AccessibilityView.Content)
			{
				return true;
			}

			if (!string.IsNullOrEmpty(AutomationProperties.GetName(panel)))
			{
				return true;
			}

			if (!string.IsNullOrEmpty(AutomationProperties.GetHelpText(panel)))
			{
				return true;
			}

			return false;
		}

		// Expose in the Content View only when the developer explicitly opted into Content via
		// AutomationProperties.IsInAccessibleTree="True" (mapped to AccessibilityView = Content).
		// Setting AutomationId alone is treated as a test-only hook (visible in the raw view
		// where UI testing libraries look) and does NOT pull the panel into the content view.
		protected override bool IsContentElementCore()
		{
			if (Owner is not Panel panel)
			{
				return false;
			}

			var accessibilityView = panel.ReadLocalValue(AutomationProperties.AccessibilityViewProperty);
			return accessibilityView is AccessibilityView.Content;
		}
	}
}
