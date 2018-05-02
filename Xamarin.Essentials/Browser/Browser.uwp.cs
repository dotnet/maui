using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task PlatformOpenAsync(Uri uri, BrowserLaunchType launchType)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return Windows.System.Launcher.LaunchUriAsync(uri).AsTask();
        }
    }
}
