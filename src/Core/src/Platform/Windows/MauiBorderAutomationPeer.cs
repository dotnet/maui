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

		// Keyboard Focusable: Allows border to receive keyboard focus
		protected override bool IsKeyboardFocusableCore() => true;

		// Control View: Contains user-interactive borders (with gesture recognizers)
		protected override bool IsControlElementCore() => true;

		// Content View: Allows screen readers to announce border structure
		protected override bool IsContentElementCore() => true;
	}
}
