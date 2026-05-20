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

		// Always expose in the Control View so that Narrator can read SemanticProperties.Description
		// even when no AutomationId is set.
		protected override bool IsControlElementCore() => true;

		// Expose in the Content View only when an AutomationId is explicitly set by the developer.
		protected override bool IsContentElementCore() => HasAutomationId();

		bool HasAutomationId()
		{
			if (Owner is not Panel panel)
			{
				return false;
			}

			return !string.IsNullOrEmpty(AutomationProperties.GetAutomationId(panel));
		}
	}
}
