using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchOptions options) =>
             Windows.System.Launcher.LaunchUriAsync(uri).AsTask();
    }
}
