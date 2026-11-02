#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

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
		public static ISecureStorage Default
		{
			get
			{
				if (Volatile.Read(ref defaultImplementation) is { } current)
					return current;

				var packageName = SecureStorageImplementation.GetDefaultPackageName();
#if WINDOWS
				var appDataDirectory = SecureStorageImplementation.UsesFileSystemAppDataDirectory
					? FileSystem.AppDataDirectory
					: null;
#endif
				return EssentialsImplementation.GetOrCreate(
					ref defaultImplementation,
					() => new SecureStorageImplementation(
						packageName
#if WINDOWS
						, appDataDirectory
#endif
						));
			}
		}

		internal static void SetDefault(ISecureStorage? implementation) =>
			EssentialsImplementation.Set(ref defaultImplementation, implementation);
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
		internal SecureStorageImplementation()
			: this(GetDefaultPackageName())
		{
		}

		internal SecureStorageImplementation(
			string packageName
#if WINDOWS
			, string? appDataDirectory = null,
			bool namespaceUnpackagedStorageByAlias = false
#endif
			)
		{
			Alias = Preferences.GetPrivatePreferencesSharedName(packageName, "preferences");
#if WINDOWS
			UnpackagedAppDataDirectory = appDataDirectory;
			NamespaceUnpackagedStorageByAlias = namespaceUnpackagedStorageByAlias;
#endif
			InitializePlatform();
		}

		internal string Alias { get; }

		internal static bool UsesFileSystemAppDataDirectory =>
#if WINDOWS
			!AppInfoUtils.IsPackagedApp;
#else
			false;
#endif

#if WINDOWS
		internal string? UnpackagedAppDataDirectory { get; }

		internal bool NamespaceUnpackagedStorageByAlias { get; }
#endif

		partial void InitializePlatform();

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

		internal static string GetDefaultPackageName()
		{
#if !NETSTANDARD && PLATFORM
			return AppInfoImplementation.GetDefaultPackageName();
#else
			return string.Empty;
#endif
		}
	}

	internal sealed class AppInfoSecureStorage : ISecureStorage
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		, IPlatformSecureStorage
#endif
	{
		readonly Lazy<SecureStorageImplementation> _implementation;
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		readonly object _sync = new();
		Security.SecAccessible _defaultAccessible;
#endif

		internal AppInfoSecureStorage(
			string packageName,
			ISecureStorage? previous
#if WINDOWS
			, string? appDataDirectory
#endif
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
			, Security.SecAccessible? inheritedDefaultAccessible = null
#endif
			)
		{
			_implementation = new(
				() =>
				{
					var implementation = new SecureStorageImplementation(
						packageName
#if WINDOWS
						, appDataDirectory,
						namespaceUnpackagedStorageByAlias: true
#endif
						);
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
					implementation.DefaultAccessible = _defaultAccessible;
#endif
					return implementation;
				},
				LazyThreadSafetyMode.ExecutionAndPublication);

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
			// This wrapper is created while the SecureStorage facade assignment locks are held.
			// Only read framework-owned predecessors here; invoking a custom platform getter would
			// execute app code under those locks. A DI-registered ISecureStorage bypasses this wrapper.
			_defaultAccessible = inheritedDefaultAccessible ?? previous switch
			{
				AppInfoSecureStorage wrapper => wrapper.DefaultAccessible,
				SecureStorageImplementation implementation => implementation.DefaultAccessible,
				_ => Security.SecAccessible.AfterFirstUnlock,
			};
#endif
		}

		internal bool IsValueCreated => _implementation.IsValueCreated;

		SecureStorageImplementation Implementation
		{
			get
			{
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
				lock (_sync)
					return _implementation.Value;
#else
				return _implementation.Value;
#endif
			}
		}

		internal string Alias => Implementation.Alias;

		public Task<string?> GetAsync(string key) =>
			Implementation.GetAsync(key);

		public Task SetAsync(string key, string value) =>
			Implementation.SetAsync(key, value);

		public bool Remove(string key) =>
			Implementation.Remove(key);

		public void RemoveAll() =>
			Implementation.RemoveAll();

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
		public Security.SecAccessible DefaultAccessible
		{
			get
			{
				lock (_sync)
					return _defaultAccessible;
			}
			set
			{
				lock (_sync)
				{
					_defaultAccessible = value;
					if (_implementation.IsValueCreated)
						_implementation.Value.DefaultAccessible = value;
				}
			}
		}

		public Task SetAsync(string key, string value, Security.SecAccessible accessible) =>
			Implementation.SetAsync(key, value, accessible);
#endif
	}
}
