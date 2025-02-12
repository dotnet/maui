using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public class MauiBorderAutomationPeer : FrameworkElementAutomationPeer
	{
		public MauiBorderAutomationPeer(Panel owner) : base(owner) { }

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
