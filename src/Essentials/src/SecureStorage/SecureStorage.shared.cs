#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Storage
{
	/// <summary>
	/// The SecureStorage API helps securely store simple key/value pairs.
	/// </summary>
	public interface ISecureStorage
	{
		/// <summary>
		/// Gets and decrypts the value for a given key.
		/// </summary>
		/// <param name="key">The key to retrieve the value for.</param>
		/// <returns>The decrypted string value or <see langword="null"/> if a value was not found.</returns>
		Task<string?> GetAsync(string key);

		/// <summary>
		/// Sets and encrypts a value for a given key.
		/// </summary>
		/// <param name="key">The key to set the value for.</param>
		/// <param name="value">Value to set.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task SetAsync(string key, string value);

		/// <summary>
		/// Removes a key and its associated value if it exists.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		bool Remove(string key);

		/// <summary>
		/// Removes all of the stored encrypted key/value pairs.
		/// </summary>
		void RemoveAll();
	}

	/// <summary>
	/// Provides abstractions for the platform specific secure storage functionality for use with <see cref="ISecureStorage"/>.
	/// </summary>
	public interface IPlatformSecureStorage
	{
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		/// <summary>
		/// Default <see cref="Security.SecAccessible"/> to use for all Get/Set calls to KeyChain.
		/// Default value is <see cref="Security.SecAccessible.AfterFirstUnlock"/>.
		/// </summary>
		Security.SecAccessible DefaultAccessible { get; set; }

		/// <summary>
		/// Sets and encrypts a value for a given key.
		/// </summary>
		/// <param name="key">The key to set the value for.</param>
		/// <param name="value">Value to set.</param>
		/// <param name="accessible">The KeyChain accessibility to create the encrypted record with.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task SetAsync(string key, string value, Security.SecAccessible accessible);
#endif
	}

	/// <summary>
	/// The SecureStorage API helps securely store simple key/value pairs.
	/// </summary>
	/// <remarks>
	/// <para>Each platform uses the platform provided APIs for storing data securely:</para>
	/// <list type="bullet">
	///   <item>
	///     <term>iOS</term><description>Data is stored in KeyChain. Additional information on SecAccessible at: <see cref="T:Security.SecAccessible" />.</description>
	///   </item>
	///   <item>
	///     <term>Android</term><description>Encryption keys are stored in KeyStore and encrypted data is stored in a named shared preference container (PackageId.microsoft.maui.essentials.preferences).</description>
	///   </item>
	///   <item>
	///     <term>Windows</term><description>Data is encrypted with DataProtectionProvider and stored in a named ApplicationDataContainer (with a container name of ApplicationId.microsoft.maui.essentials.preferences).</description>
	///   </item>
	/// </list>
	/// <para>NOTE: On Android devices running below API 23 (6.0 Marshmallow) there is no AES available in KeyStore.  As a best practice this API will generate an RSA/ECB/PKCS7Padding key pair stored in KeyStore (the only type supported in KeyStore by these lower API levels), which is used to wrap an AES key generated at runtime.  This wrapped key is stored in Preferences.</para>
	/// </remarks>
	public static partial class SecureStorage
	{
		/// <summary>
		/// Gets and decrypts the value for a given key.
		/// </summary>
		/// <param name="key">The key to retrieve the value for.</param>
		/// <returns>The decrypted string value or <see langword="null"/> if a value was not found.</returns>
		public static Task<string?> GetAsync(string key) =>
			Current.GetAsync(key);

		/// <summary>
		/// Sets and encrypts a value for a given key.
		/// </summary>
		/// <param name="key">The key to set the value for.</param>
		/// <param name="value">Value to set.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task SetAsync(string key, string value) =>
			Current.SetAsync(key, value);

		/// <summary>
		/// Removes a key and its associated value if it exists.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		public static bool Remove(string key) =>
			Current.Remove(key);

		/// <summary>
		/// Removes all of the stored encrypted key/value pairs.
		/// </summary>
		public static void RemoveAll() =>
			Current.RemoveAll();

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		/// <summary>
		/// Default <see cref="Security.SecAccessible"/> to use for all Get/Set calls to KeyChain.
		/// Default value is <see cref="Security.SecAccessible.AfterFirstUnlock"/>.
		/// </summary>
		public static Security.SecAccessible DefaultAccessible
		{
			get => Current.GetDefaultAccessible();
			set => Current.SetDefaultAccessible(value);
		}

		/// <summary>
		/// Sets and encrypts a value for a given key.
		/// </summary>
		/// <param name="key">The key to set the value for.</param>
		/// <param name="value">Value to set.</param>
		/// <param name="accessible">The KeyChain accessibility to create the encrypted record with.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task SetAsync(string key, string value, Security.SecAccessible accessible) =>
			Current.SetAsync(key, value, accessible);
#endif

		static ISecureStorage Current => Storage.SecureStorage.Default;

		static ISecureStorage? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static ISecureStorage Default =>
			defaultImplementation ??= new SecureStorageImplementation();

		internal static void SetDefault(ISecureStorage? implementation)
		{
			defaultImplementation = implementation;
		}
	}

	/// <summary>
	/// This class contains static extension methods for use with <see cref="ISecureStorage"/>.
	/// </summary>
	public static class SecureStorageExtensions
	{
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		/// <summary>
		/// Gets the default KeyChain accessibility used to encrypt data.
		/// </summary>
		/// <param name="secureStorage">The object this method is invoked on.</param>
		/// <returns>The current default <see cref="Security.SecAccessible"/> value.</returns>
		/// <exception cref="PlatformNotSupportedException">Thrown when <paramref name="secureStorage"/> does not implement <see cref="IPlatformSecureStorage"/>.</exception>
		public static Security.SecAccessible GetDefaultAccessible(this ISecureStorage secureStorage)
		{
			if (secureStorage is not IPlatformSecureStorage platform)
				throw new PlatformNotSupportedException("This implementation of ISecureStorage does not implement IPlatformSecureStorage.");

			return platform.DefaultAccessible;
		}

		/// <summary>
		/// Sets the default KeyChain accessibility used to encrypt data.
		/// </summary>
		/// <param name="secureStorage">The object this method is invoked on.</param>
		/// <param name="accessible">The default KeyChain accessibility to set.</param>
		/// <returns>The current default <see cref="Security.SecAccessible"/> value.</returns>
		/// <exception cref="PlatformNotSupportedException">Thrown when <paramref name="secureStorage"/> does not implement <see cref="IPlatformSecureStorage"/>.</exception>
		public static void SetDefaultAccessible(this ISecureStorage secureStorage, Security.SecAccessible accessible)
		{
			if (secureStorage is not IPlatformSecureStorage platform)
				throw new PlatformNotSupportedException("This implementation of ISecureStorage does not implement IPlatformSecureStorage.");

			platform.DefaultAccessible = accessible;
		}

		/// <summary>
		/// Sets and encrypts a value for a given key.
		/// </summary>
		/// <param name="secureStorage">The object this method is invoked on.</param>
		/// <param name="key">The key to set the value for.</param>
		/// <param name="value">Value to set.</param>
		/// <param name="accessible">The KeyChain accessibility to create the encrypted record with.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
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

		public Task<string?> GetAsync(string key)
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
