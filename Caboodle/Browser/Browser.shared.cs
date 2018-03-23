using System;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class Browser
    {
        public static Task OpenAsync(string uri) =>
            OpenAsync(uri, BrowserLaunchType.SystemPreferred);

        public static Task OpenAsync(string uri, BrowserLaunchType launchType)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri), $"Uri cannot be empty.");
            }

            return OpenAsync(new Uri(uri), launchType);
        }

        public static Task OpenAsync(Uri uri) =>
          OpenAsync(uri, BrowserLaunchType.SystemPreferred);
    }

    public enum BrowserLaunchType
    {
        External,
        SystemPreferred
    }
}
