using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace Microsoft.Caboodle
{
    public static partial class Clipboard
    {
        static ClipboardManager clipboardManager;

        static ClipboardManager ClipboardManager
        {
            get
            {
                if (clipboardManager == null || clipboardManager.Handle == IntPtr.Zero)
                    clipboardManager = (ClipboardManager)Application.Context.GetSystemService(Context.ClipboardService);

                return clipboardManager;
            }
        }

        public static Task SetTextAsync(string text) =>
            Platform.InvokeOnMainThread(() => ClipboardManager.PrimaryClip = ClipData.NewPlainText("Text", text));

        public static bool HasText => ClipboardManager.HasPrimaryClip;

        public static Task<string> GetTextAsync() => HasText
            ? Platform.InvokeOnMainThread(() => clipboardManager.PrimaryClip.GetItemAt(0).Text)
            : Task.FromResult<string>(null);
    }
}
