#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Storage
{
	public interface ISecureStorage
	{
		Task<string> GetAsync(string key);

		Task SetAsync(string key, string value);

		bool Remove(string key);

		void RemoveAll();
	}

	public interface IPlatformSecureStorage
	{
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		Security.SecAccessible DefaultAccessible { get; set; }

		Task SetAsync(string key, string value, Security.SecAccessible accessible);
#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SecureStorage']/Docs/*" />
	public static partial class SecureStorage
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='GetAsync'][1]/Docs/*" />
		public static Task<string> GetAsync(string key) =>
			Current.GetAsync(key);

		public static Task SetAsync(string key, string value) =>
			Current.SetAsync(key, value);

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='Remove'][1]/Docs/*" />
		public static bool Remove(string key) =>
			Current.Remove(key);

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='RemoveAll'][1]/Docs/*" />
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

		static ISecureStorage? defaultImplementation;

		public static ISecureStorage Default =>
			defaultImplementation ??= new SecureStorageImplementation();

		internal static void SetDefault(ISecureStorage? implementation)
		{
			defaultImplementation = implementation;
		}
	}

	public static class SecureStorageExtensions
	{
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		public static Security.SecAccessible GetDefaultAccessible(this ISecureStorage secureStorage)
		{
			if (secureStorage is not IPlatformSecureStorage platform)
				throw new PlatformNotSupportedException("This implementation of ISecureStorage does not implement IPlatformSecureStorage.");

			return platform.DefaultAccessible;
		}
		public static void SetDefaultAccessible(this ISecureStorage secureStorage, Security.SecAccessible accessible)
		{
			if (secureStorage is not IPlatformSecureStorage platform)
				throw new PlatformNotSupportedException("This implementation of ISecureStorage does not implement IPlatformSecureStorage.");

			platform.DefaultAccessible = accessible;
		}

		public static Task SetAsync(this ISecureStorage secureStorage, string key, string value, Security.SecAccessible accessible)
		{
			if (secureStorage is not IPlatformSecureStorage platform)
				throw new PlatformNotSupportedException("This implementation of ISecureStorage does not implement IPlatformSecureStorage.");

			return platform.SetAsync(key, value, accessible);
		}
#endif
	}

	partial class SecureStorageImplementation
	{
#if !NETSTANDARD && PLATFORM
		// Special Alias that is only used for Secure Storage. All others should use: Preferences.GetPrivatePreferencesSharedName
		internal static readonly string Alias = Preferences.GetPrivatePreferencesSharedName("preferences");
#endif

		public Task<string> GetAsync(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return PlatformGetAsync(key);
		}

		public Task SetAsync(string key, string value)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return PlatformSetAsync(key, value);
		}

		public bool Remove(string key)
			=> PlatformRemove(key);

		public void RemoveAll()
			=> PlatformRemoveAll();
	}
}
