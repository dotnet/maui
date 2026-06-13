#nullable enable
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public partial class MauiLayoutAutomationPeer : FrameworkElementAutomationPeer
	{
		public MauiLayoutAutomationPeer(LayoutPanel owner) : base(owner) { }

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

		// Layouts with accessibility semantics use Pane to signal a grouping/container element.
		// AutomationId-only layouts remain discoverable to Windows UI automation, but report as
		// Custom so screen readers do not announce them as meaningful groups.
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return HasAccessibilitySemantics()
				? AutomationControlType.Pane
				: AutomationControlType.Custom;
		}

		protected override string GetLocalizedControlTypeCore()
		{
			return HasAccessibilitySemantics()
				? base.GetLocalizedControlTypeCore() ?? string.Empty
				: string.Empty;
		}

		// Layouts are never keyboard-focusable — they are structural containers, not interactive elements.
		protected override bool IsKeyboardFocusableCore() => false;

		// Layouts/panels are structural containers. By default we exclude anonymous layouts from
		// the UIA Control view so screen readers (Narrator, NVDA) don't stop on every nested Grid /
		// StackLayout while a user is navigating a page.
		//
		// We DO opt in to the Control view when the developer signals that this panel should
		// be discoverable through UI Automation:
		//   * AutomationId on the cross-platform view, so Windows Appium/WinAppDriver can find
		//     layout containers by their test hook. This is a deliberate trade-off: Windows UI
		//     test automation does not see these peers unless they participate in Control view,
		//     so named layout containers become visible to screen readers too.
		//   * AutomationProperties.IsInAccessibleTree="True" on the cross-platform view
		//     (mapped to AccessibilityView = Content on the platform view; Control can still be
		//     set directly by platform code), or
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

			// Explicit opt-out wins over any AutomationId or semantic opt-in signal.
			if (accessibilityView is AccessibilityView.Raw)
			{
				return false;
			}
			else if (accessibilityView is AccessibilityView.Control or AccessibilityView.Content)
			{
				return true;
			}

			if (!string.IsNullOrWhiteSpace(AutomationProperties.GetAutomationId(panel)))
			{
				return true;
			}

			if (!string.IsNullOrWhiteSpace(AutomationProperties.GetName(panel)))
			{
				return true;
			}

			if (!string.IsNullOrWhiteSpace(AutomationProperties.GetHelpText(panel)))
			{
				return true;
			}

			return false;
		}

		// Expose in the Content View only when the developer explicitly opted into Content via
		// AutomationProperties.IsInAccessibleTree="True" (mapped to AccessibilityView = Content).
		// AutomationId keeps the panel discoverable to Windows UI test automation through the
		// Control view, but does NOT pull the panel into the Content view.
		protected override bool IsContentElementCore()
		{
			if (Owner is not Panel panel)
			{
				return false;
			}

			var accessibilityView = panel.ReadLocalValue(AutomationProperties.AccessibilityViewProperty);
			return accessibilityView is AccessibilityView.Content;
		}

		bool HasAccessibilitySemantics()
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

			return !string.IsNullOrWhiteSpace(AutomationProperties.GetName(panel)) ||
				!string.IsNullOrWhiteSpace(AutomationProperties.GetHelpText(panel));
		}
	}
}
