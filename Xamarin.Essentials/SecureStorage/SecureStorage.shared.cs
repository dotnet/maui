using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class SecureStorage
    {
        internal static readonly string Alias = Preferences.PrivatePreferencesSharedName;

        public static Task<string> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return PlatformGetAsync(key);
        }

        public static Task SetAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return PlatformSetAsync(key, value);
        }
    }
}
