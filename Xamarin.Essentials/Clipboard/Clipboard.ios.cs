using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        static Task PlatformSetTextAsync(string text)
        {
            UIPasteboard.General.String = text;
            return Task.CompletedTask;
        }

        static bool PlatformHasText
            => UIPasteboard.General.HasStrings;

        static Task<string> PlatformGetTextAsync()
            => Task.FromResult(UIPasteboard.General.String);
    }
}
