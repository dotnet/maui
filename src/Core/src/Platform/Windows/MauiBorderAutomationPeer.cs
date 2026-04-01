using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	// TODO: Make this class public in .NET11.0. Issue Link: https://github.com/dotnet/maui/issues/30205
	internal partial class MauiBorderAutomationPeer : FrameworkElementAutomationPeer
	{
		internal MauiBorderAutomationPeer(Panel owner) : base(owner) { }

		// Control Type: Returns "Pane" as the control type for the border automation peer
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Pane;
		}

		// Class Name: Returns "Panel" as the class name for the border automation peer
		protected override string GetClassNameCore()
		{
			return nameof(Panel);
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
