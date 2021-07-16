using Foundation;
using UIKit;

namespace Microsoft.Maui.Semantics
{
	public class SemanticService
	{
		public void SetAnnouncement(string text)
		{
			if (!UIAccessibility.IsVoiceOverRunning)
				return;

			UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(text));
		}
	}
}
