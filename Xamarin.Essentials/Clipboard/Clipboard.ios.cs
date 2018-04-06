using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        public static void SetText(string text)
            => UIPasteboard.General.String = text;

        public static bool HasText
            => UIPasteboard.General.HasStrings;

        public static Task<string> GetTextAsync()
            => Task.FromResult(UIPasteboard.General.String);
    }
}
