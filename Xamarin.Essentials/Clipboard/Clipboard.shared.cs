using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        public static void SetText(string text)
            => PlatformSetText(text);

        public static bool HasText
            => PlatformHasText;

        public static Task<string> GetTextAsync()
            => PlatformGetTextAsync();
    }
}
