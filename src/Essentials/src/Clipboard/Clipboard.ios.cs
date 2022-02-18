using System;
using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class ClipboardImplementation : IClipboard
	{
		public Task SetTextAsync(string text)
		{
			UIPasteboard.General.String = text;
			return Task.CompletedTask;
		}

		NSObject observer;

		public bool HasText
			=> UIPasteboard.General.HasStrings;

		public Task<string> GetTextAsync()
			=> Task.FromResult(UIPasteboard.General.String);

		public void StartClipboardListeners()
		{
			observer = NSNotificationCenter.DefaultCenter.AddObserver(
				UIPasteboard.ChangedNotification,
				ClipboardChangedObserver);
		}

		public void StopClipboardListeners()
			=> NSNotificationCenter.DefaultCenter.RemoveObserver(observer);

		public void ClipboardChangedObserver(NSNotification notification)
			=> Clipboard.ClipboardChangedInternal();
	}
}
