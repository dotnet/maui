using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class SecureStorage
    {
        static string Alias =>
            $"{AppInfo.PackageName}.caboodle";

        public static Task<string> GetAsync(string key)
            => PlatformGetAsync(key);

        public static Task SetAsync(string key, string value)
            => PlatformSetAsync(key, value);
    }
}
