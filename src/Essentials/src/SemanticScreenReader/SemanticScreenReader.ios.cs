using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Accessibility
{
	partial class SemanticScreenReaderImplementation : ISemanticScreenReader
	{
		public void Announce(string text)
		{
			if (!UIAccessibility.IsVoiceOverRunning)
				return;

			ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
			{
				await Task.Delay(100);
				UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, new NSString(text));
			});
		}
	}
}
