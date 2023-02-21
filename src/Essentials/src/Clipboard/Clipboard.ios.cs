#nullable enable
using System;
using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ClipboardImplementation : IClipboard
	{
		NSObject? observer;

		public Task SetTextAsync(string? text)
		{
			UIPasteboard.General.String = text;
			return Task.CompletedTask;
		}

		public bool HasText
			=> UIPasteboard.General.HasStrings;

		public Task<string?> GetTextAsync()
			=> Task.FromResult(UIPasteboard.General.String);

		void StartClipboardListeners()
		{
			observer = NSNotificationCenter.DefaultCenter.AddObserver(
				UIPasteboard.ChangedNotification,
				ClipboardChangedObserver);
		}

		void StopClipboardListeners()
		{
			if (observer is not null)
				NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
		}

		/// <summary>
		/// The observer for triggering the <see cref="ClipboardContentChanged"/> event.
		/// </summary>
		/// <param name="notification">The notification that triggered this event.</param>
		public void ClipboardChangedObserver(NSNotification notification)
			=> OnClipboardContentChanged();
	}
}
