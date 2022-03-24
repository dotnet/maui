using System;
using System.Threading.Tasks;
using Android.Content;
using static Android.Content.ClipboardManager;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	public partial class ClipboardImplementation : IClipboard
	{
		static readonly Lazy<ClipboardChangeListener> clipboardListener
			= new Lazy<ClipboardChangeListener>(() => new ClipboardChangeListener());

		public Task SetTextAsync(string text)
		{
			Platform.ClipboardManager.PrimaryClip = ClipData.NewPlainText("Text", text);
			return Task.CompletedTask;
		}

		public bool HasText
			=> Platform.ClipboardManager.HasPrimaryClip && !string.IsNullOrEmpty(Platform.ClipboardManager.PrimaryClip?.GetItemAt(0)?.Text);

		public Task<string> GetTextAsync()
			=> Task.FromResult(Platform.ClipboardManager.PrimaryClip?.GetItemAt(0)?.Text);

		public void StartClipboardListeners()
			=> Platform.ClipboardManager.AddPrimaryClipChangedListener(clipboardListener.Value);

		public void StopClipboardListeners()
			=> Platform.ClipboardManager.RemovePrimaryClipChangedListener(clipboardListener.Value);
	}

	class ClipboardChangeListener : Java.Lang.Object, IOnPrimaryClipChangedListener
	{
		void IOnPrimaryClipChangedListener.OnPrimaryClipChanged() =>
			Clipboard.ClipboardChangedInternal();
	}
}
