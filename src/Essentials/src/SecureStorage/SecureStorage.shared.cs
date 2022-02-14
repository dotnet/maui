#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public interface ISecureStorage
	{
		Task<string> GetAsync(string key);
		Task SetAsync(string key, string value);
		bool Remove(string key);
		void RemoveAll();
	}

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

			return Current.GetAsync(key);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='SetAsync'][0]/Docs" />
		public static Task SetAsync(string key, string value)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return Current.SetAsync(key, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='Remove']/Docs" />
		public static bool Remove(string key)
			=> Current.Remove(key);

		/// <include file="../../docs/Microsoft.Maui.Essentials/SecureStorage.xml" path="//Member[@MemberName='RemoveAll']/Docs" />
		public static void RemoveAll()
			=> Current.RemoveAll();

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
