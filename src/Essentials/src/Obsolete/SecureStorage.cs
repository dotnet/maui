#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SecureStorage']/Docs" />
	public static partial class SecureStorage
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='GetAsync'][1]/Docs" />
		public static Task<string> GetAsync(string key) =>
			Current.GetAsync(key);

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='SetAsync'][0 and position()=0]/Docs" />
		public static Task SetAsync(string key, string value) =>
			Current.SetAsync(key, value);

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='Remove'][1]/Docs" />
		public static bool Remove(string key) =>
			Current.Remove(key);

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='RemoveAll'][1]/Docs" />
		public static void RemoveAll() =>
			Current.RemoveAll();

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		public static Security.SecAccessible DefaultAccessible
		{
			get => Current.GetDefaultAccessible();
			set => Current.SetDefaultAccessible(value);
		}

		public static Task SetAsync(string key, string value, Security.SecAccessible accessible) =>
			Current.SetAsync(key, value, accessible);
#endif

		static ISecureStorage Current => Storage.SecureStorage.Default;
	}
}
