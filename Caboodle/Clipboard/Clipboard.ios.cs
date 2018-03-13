using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Caboodle
{
    public static partial class Clipboard
    {
        public static Task SetTextAsync(string text) =>
            Platform.InvokeOnMainThread(() => UIPasteboard.General.String = text);

        public static bool HasText => UIPasteboard.General.HasStrings;

        public static Task<string> GetTextAsync() =>
            Platform.InvokeOnMainThread(() => UIPasteboard.General.String);
    }
}
