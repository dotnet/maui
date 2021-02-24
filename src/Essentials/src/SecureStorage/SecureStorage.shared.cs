using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
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
	}
}
