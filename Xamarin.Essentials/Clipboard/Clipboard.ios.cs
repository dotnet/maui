using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        static void PlatformSetText(string text)
            => UIPasteboard.General.String = text;

        static bool PlatformHasText
            => UIPasteboard.General.HasStrings;

        static Task<string> PlatformGetTextAsync()
            => Task.FromResult(UIPasteboard.General.String);
    }
}
