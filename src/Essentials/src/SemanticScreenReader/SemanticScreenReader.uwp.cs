

using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Essentials
{
	public static partial class SemanticScreenReader
	{
		static void PlatformAnnounce(string text)
		{
			if (Platform.CurrentWindow == null)
				return;

			var peer = FrameworkElementAutomationPeer.FromElement(Platform.CurrentWindow.Content);
			peer.RaiseNotificationEvent(
				AutomationNotificationKind.ActionAborted,
				AutomationNotificationProcessing.ImportantMostRecent,
				text,
				"270FA098-C644-40A2-A0BE-A9BEA1222A1E");
		}
	}
}
