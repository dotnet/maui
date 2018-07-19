using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
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

        public static Task OpenAsync(Uri uri, BrowserLaunchType launchType) =>
            PlatformOpenAsync(EscapeUri(uri), launchType);

        internal static Uri EscapeUri(Uri uri)
        {
#if NETSTANDARD1_0
            return uri;
#else
            var idn = new System.Globalization.IdnMapping();
            return new Uri(uri.Scheme + "://" + idn.GetAscii(uri.DnsSafeHost) + uri.PathAndQuery);
#endif
        }
    }

    public enum BrowserLaunchType
    {
        External,
        SystemPreferred
    }
}
