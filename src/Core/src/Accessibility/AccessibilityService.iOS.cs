using Foundation;
using UIKit;

namespace Microsoft.Maui.Accessibility
{
	public class AccessibilityService
	{
		public void SetAnnouncement(string text)
		{
			if (!UIAccessibility.IsVoiceOverRunning)
				return;

			UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(text));
		}
	}
}
