using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class SecureStorage
    {
        static string Alias =>
            $"{AppInfo.PackageName}.Xamarin.Essentials";

        public static Task<string> GetAsync(string key)
            => PlatformGetAsync(key);

        public static Task SetAsync(string key, string value)
            => PlatformSetAsync(key, value);
    }
}
