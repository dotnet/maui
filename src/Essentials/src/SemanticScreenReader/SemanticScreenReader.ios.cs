using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class SemanticScreenReader
	{
		static void PlatformAnnounce(string text)
		{
			if (!UIAccessibility.IsVoiceOverRunning)
				return;

			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await Task.Delay(100);
				UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(text));
			});
			
		}
	}
}
