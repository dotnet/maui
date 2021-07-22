using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public class SemanticService : ISemanticService
	{
		public void Announce(string text)
		{
			if (!UIAccessibility.IsVoiceOverRunning)
				return;

			UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(text));
		}
	}
}
