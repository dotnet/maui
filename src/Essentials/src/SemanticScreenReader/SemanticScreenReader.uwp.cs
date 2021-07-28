

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Essentials
{
	public static partial class SemanticScreenReader
	{
		static void PlatformAnnounce(string text)
		{
			if (Platform.CurrentWindow == null)
				return;

			var peer = FindAutomationPeer(Platform.CurrentWindow.Content);
			peer.RaiseNotificationEvent(
				AutomationNotificationKind.ActionAborted,
				AutomationNotificationProcessing.ImportantMostRecent,
				text,
				"270FA098-C644-40A2-A0BE-A9BEA1222A1E");
		}

		static AutomationPeer FindAutomationPeer(DependencyObject depObj)
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child is UIElement element && FrameworkElementAutomationPeer.FromElement(element) != null)
					{
						return FrameworkElementAutomationPeer.FromElement(element);
					}

					var childItem = FindAutomationPeer(child);
					if (childItem != null)
						return childItem;
				}
			}
			return null;
		}
	}
}
