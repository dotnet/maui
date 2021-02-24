using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class Clipboard
	{
		static Task PlatformSetTextAsync(string text)
		{
			UIPasteboard.General.String = text;
			return Task.CompletedTask;
		}

		static NSObject observer;

		static bool PlatformHasText
			=> UIPasteboard.General.HasStrings;

		static Task<string> PlatformGetTextAsync()
			=> Task.FromResult(UIPasteboard.General.String);

		static void StartClipboardListeners()
		{
			observer = NSNotificationCenter.DefaultCenter.AddObserver(
				UIPasteboard.ChangedNotification,
				ClipboardChangedObserver);
		}

		static void StopClipboardListeners()
			=> NSNotificationCenter.DefaultCenter.RemoveObserver(observer);

		static void ClipboardChangedObserver(NSNotification notification)
			=> ClipboardChangedInternal();
	}
}
