using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        static Task PlatformSetTextAsync(string text)
            => throw new System.PlatformNotSupportedException();

        static bool PlatformHasText
            => throw new System.PlatformNotSupportedException();

        static Task<string> PlatformGetTextAsync()
            => throw new System.PlatformNotSupportedException();
    }
}
