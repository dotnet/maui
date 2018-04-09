using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class SecureStorage
    {
        internal static readonly string Alias = Preferences.PrivatePreferencesSharedName;

        public static Task<string> GetAsync(string key)
            => PlatformGetAsync(key);

        public static Task SetAsync(string key, string value)
            => PlatformSetAsync(key, value);
    }
}
