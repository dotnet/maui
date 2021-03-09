using System;
using System.Threading.Tasks;
using Android.Content;
using static Android.Content.ClipboardManager;

namespace Microsoft.Maui.Essentials
{
	public static partial class Clipboard
	{
		static readonly Lazy<ClipboardChangeListener> clipboardListener
			= new Lazy<ClipboardChangeListener>(() => new ClipboardChangeListener());

		static Task PlatformSetTextAsync(string text)
		{
			Platform.ClipboardManager.PrimaryClip = ClipData.NewPlainText("Text", text);
			return Task.CompletedTask;
		}

		static bool PlatformHasText
			=> Platform.ClipboardManager.HasPrimaryClip && !string.IsNullOrEmpty(Platform.ClipboardManager.PrimaryClip?.GetItemAt(0)?.Text);

		static Task<string> PlatformGetTextAsync()
			=> Task.FromResult(Platform.ClipboardManager.PrimaryClip?.GetItemAt(0)?.Text);

		static void StartClipboardListeners()
			=> Platform.ClipboardManager.AddPrimaryClipChangedListener(clipboardListener.Value);

		static void StopClipboardListeners()
			=> Platform.ClipboardManager.RemovePrimaryClipChangedListener(clipboardListener.Value);
	}

	class ClipboardChangeListener : Java.Lang.Object, IOnPrimaryClipChangedListener
	{
		void IOnPrimaryClipChangedListener.OnPrimaryClipChanged() =>
			Clipboard.ClipboardChangedInternal();
	}
}
