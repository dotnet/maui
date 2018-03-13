using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace Microsoft.Caboodle
{
    public static partial class Clipboard
    {
        static ClipboardManager ClipboardManager
            => (ClipboardManager)Application.Context.GetSystemService(Context.ClipboardService);

        public static void SetText(string text)
            => ClipboardManager.PrimaryClip = ClipData.NewPlainText("Text", text);

        public static bool HasText
            => ClipboardManager.HasPrimaryClip;

        public static Task<string> GetTextAsync()
            => Task.FromResult(ClipboardManager.PrimaryClip?.GetItemAt(0)?.Text);
    }
}
