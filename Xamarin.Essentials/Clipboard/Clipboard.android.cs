using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        public static void SetText(string text)
            => Platform.ClipboardManager.PrimaryClip = ClipData.NewPlainText("Text", text);

        public static bool HasText
            => Platform.ClipboardManager.HasPrimaryClip;

        public static Task<string> GetTextAsync()
            => Task.FromResult(Platform.ClipboardManager.PrimaryClip?.GetItemAt(0)?.Text);
    }
}
