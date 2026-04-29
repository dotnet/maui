using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	// TODO: Make this class public in .NET11.0. Issue Link: https://github.com/dotnet/maui/issues/30205
	internal partial class MauiBorderAutomationPeer : FrameworkElementAutomationPeer
	{
		internal MauiBorderAutomationPeer(Panel owner) : base(owner) { }

		// Control Type: Returns "Custom" when the border is interactive (has tap gestures / IsTabStop),
		// so screen readers announce it as actionable. Returns "Pane" for non-interactive borders.
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			if ((Owner as ContentPanel)?.IsTabStop == true)
			{
				return AutomationControlType.Custom;
			}

			return AutomationControlType.Pane;
		}

		// Class Name: Returns "Panel" as the class name for the border automation peer
		protected override string GetClassNameCore()
		{
			return nameof(Panel);
		}

		// Localized Control Type: When the border is interactive, screen readers announce it as "border"
		// instead of the generic "custom" or "pane" text, giving users context about the element.
		protected override string GetLocalizedControlTypeCore()
		{
			if ((Owner as ContentPanel)?.IsTabStop == true)
			{
				// TODO: Route through a .resw resource file for localization when MAUI's localization infra supports it.
				return "border";
			}

			return base.GetLocalizedControlTypeCore();
		}

		// Keyboard Focusable: Allows border to receive keyboard focus only when it has gesture recognizers (IsTabStop = true)
		protected override bool IsKeyboardFocusableCore() => (Owner as ContentPanel)?.IsTabStop == true;

		// Control View: Expose only when the Border is interactive (has tap gestures) or has an explicit AutomationId
		protected override bool IsControlElementCore() => IsInteractiveOrNamed();

		// Content View: Expose only when the Border is interactive (has tap gestures) or has an explicit AutomationId
		protected override bool IsContentElementCore() => IsInteractiveOrNamed();

		// Gates automation exposure: Border appears in the control/content view only when it is interactive
		// (IsTabStop == true, set by GesturePlatformManager when tap gestures are present) or explicitly
		// named via AutomationId (opted-in for automation visibility by the developer).
		bool IsInteractiveOrNamed()
		{
			if (Owner is not ContentPanel contentPanel)
			{
				return false;
			}

			return contentPanel.IsTabStop || !string.IsNullOrEmpty(AutomationProperties.GetAutomationId(contentPanel));
		}
	}
}
