using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        public static Task SetTextAsync(string text)
            => PlatformSetTextAsync(text);

        public static bool HasText
            => PlatformHasText;

        public static Task<string> GetTextAsync()
            => PlatformGetTextAsync();
    }
}
