using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	// TODO: Make this class public in .NET11.0. Issue Link: https://github.com/dotnet/maui/issues/30205
	internal partial class MauiBorderAutomationPeer : FrameworkElementAutomationPeer
	{
		internal MauiBorderAutomationPeer(ContentPanel owner) : base(owner) { }

		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Pane;
		}

		protected override string GetClassNameCore()
		{
			if (Owner is ContentPanel panel)
			{
				return panel.CrossPlatformLayout?.GetType().Name ?? nameof(Panel);
			}

			return nameof(Panel);
		}

		protected override bool IsControlElementCore() => true;

		protected override bool IsContentElementCore() => HasAutomationId();

		bool HasAutomationId()
		{
			if (Owner is not ContentPanel contentPanel)
			{
				return false;
			}

			return !string.IsNullOrEmpty(AutomationProperties.GetAutomationId(contentPanel));
		}
	}
}
