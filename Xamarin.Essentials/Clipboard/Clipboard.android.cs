using System;
using System.Threading.Tasks;
using Android.Content;

namespace Xamarin.Essentials
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
            => Platform.ClipboardManager.HasPrimaryClip;

        static Task<string> PlatformGetTextAsync()
            => Task.FromResult(Platform.ClipboardManager.PrimaryClip?.GetItemAt(0)?.Text);

        static void StartClipboardListeners()
            => Platform.ClipboardManager.AddPrimaryClipChangedListener(clipboardListener.Value);

        static void StopClipboardListeners()
            => Platform.ClipboardManager.RemovePrimaryClipChangedListener(clipboardListener.Value);
    }

    public class ClipboardChangeListener : Java.Lang.Object, ClipboardManager.IOnPrimaryClipChangedListener
    {
        public void OnPrimaryClipChanged()
        {
            Clipboard.ClipboardChangedInternal();
        }
    }
}
