using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SecureStorage']/Docs" />
	public static partial class SecureStorage
	{
		// Special Alias that is only used for Secure Storage. All others should use: Preferences.GetPrivatePreferencesSharedName
		internal static readonly string Alias = Preferences.GetPrivatePreferencesSharedName("preferences");

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='GetAsync']/Docs" />
		public static Task<string> GetAsync(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return PlatformGetAsync(key);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='SetAsync'][0]/Docs" />
		public static Task SetAsync(string key, string value)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return PlatformSetAsync(key, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='Remove']/Docs" />
		public static bool Remove(string key)
			=> PlatformRemove(key);

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='RemoveAll']/Docs" />
		public static void RemoveAll()
			=> PlatformRemoveAll();
	}
}
