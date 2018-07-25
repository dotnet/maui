using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        public static Task<bool> CanOpenAsync(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));

            return PlatformCanOpenAsync(new Uri(uri));
        }

        public static Task<bool> CanOpenAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return PlatformCanOpenAsync(uri);
        }

        public static Task OpenAsync(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));

            return PlatformOpenAsync(new Uri(uri));
        }

        public static Task OpenAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return PlatformOpenAsync(uri);
        }
    }
}
