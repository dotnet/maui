using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class SecureStorage
    {
        // Special Alias that is only used for Secure Storage. All others should use: Preferences.GetPrivatePreferencesSharedName
        internal static readonly string Alias = $"{AppInfo.PackageName}.xamarinessentials";

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

        public static bool Remove(string key)
            => PlatformRemove(key);

        public static void RemoveAll()
            => PlatformRemoveAll();

        public static string Crc64(string value)
            => Crc64(System.Text.Encoding.UTF8.GetBytes(value));

        public static string Crc64(byte[] data)
        {
#if NETSTANDARD1_0
            throw ExceptionUtils.NotSupportedOrImplementedException;
#else
            static char GetHexValue(int i)
                => (char)(i < 10 ? i + 48 : i - 10 + 65);

            var crc = new Crc64();
            var hashedData = crc.ComputeHash(data);

            var array = new char[hashedData.Length * 2];
            for (int i = 0, j = 0; i < hashedData.Length; i += 1, j += 2)
            {
                var b = hashedData[i];
                array[j] = GetHexValue(b / 16);
                array[j + 1] = GetHexValue(b % 16);
            }
            return new string(array);
#endif
        }
    }
}
