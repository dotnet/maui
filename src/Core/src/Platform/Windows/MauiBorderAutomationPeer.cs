using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	// TODO: Make this class public in .NET 10.0. Issue Link: https://github.com/dotnet/maui/issues/30205
	internal class MauiBorderAutomationPeer : FrameworkElementAutomationPeer
	{
		internal MauiBorderAutomationPeer(Panel owner) : base(owner) { }

		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Pane;
		}

		protected override string GetClassNameCore()
		{
			return nameof(Panel);
		}

		protected override bool IsControlElementCore() => true;

		protected override bool IsContentElementCore() => true;
	}
}
