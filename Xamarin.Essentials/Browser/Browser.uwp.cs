using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        public static Task OpenAsync(Uri uri, BrowserLaunchType launchType)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri), "Uri cannot be null.");
            }

            return Windows.System.Launcher.LaunchUriAsync(uri).AsTask();
        }
    }
}
