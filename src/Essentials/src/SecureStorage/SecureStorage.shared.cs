#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
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

	/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.SecureStorage']/Docs" />
	public static partial class SecureStorage
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='GetAsync'][1]/Docs" />
		public static Task<string> GetAsync(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return Current.GetAsync(key);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='SetAsync'][0 and position()=0]/Docs" />
		public static Task SetAsync(string key, string value)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return Current.SetAsync(key, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='Remove'][1]/Docs" />
		public static bool Remove(string key)
			=> Current.Remove(key);

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='RemoveAll'][1]/Docs" />
		public static void RemoveAll()
			=> Current.RemoveAll();

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		public static Security.SecAccessible DefaultAccessible
		{
			get
			{ 
				if (Current is IPlatformSecureStorage p)
				{
					return p.DefaultAccessible;
				}

				throw new NotImplementedException();
			}
			set
			{
				if (Current is IPlatformSecureStorage p)
				{
					p.DefaultAccessible = value;
				}

				throw new NotImplementedException();
			}
		}

		public static Task SetAsync(string key, string value, Security.SecAccessible accessible)
		{
			if (Current is IPlatformSecureStorage p)
			{
				return p.SetAsync(key, value, accessible);
			}

			throw new NotImplementedException();
		}
#endif

		static ISecureStorage? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static ISecureStorage Current =>
			currentImplementation ??= new SecureStorageImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(ISecureStorage? implementation)
		{
			currentImplementation = implementation;
		}
	}
}

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class SecureStorageImplementation
	{
#if !NETSTANDARD
		// Special Alias that is only used for Secure Storage. All others should use: Preferences.GetPrivatePreferencesSharedName
		internal static readonly string Alias = Preferences.GetPrivatePreferencesSharedName("preferences");
#endif

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='GetAsync'][2]/Docs" />
		public Task<string> GetAsync(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			return PlatformGetAsync(key);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='SetAsync'][0 and position()=1]/Docs" />
		public Task SetAsync(string key, string value)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return PlatformSetAsync(key, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='Remove'][2]/Docs" />
		public bool Remove(string key)
			=> PlatformRemove(key);

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='RemoveAll'][2]/Docs" />
		public void RemoveAll()
			=> PlatformRemoveAll();

	}
}
