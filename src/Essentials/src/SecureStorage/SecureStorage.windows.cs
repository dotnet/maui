using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using SecureStorageDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, byte[]>;

namespace Microsoft.Maui.Storage
{
	partial class SecureStorageImplementation : ISecureStorage
	{
		ISecureStorageImplementation _secureStorage = null!;

		partial void InitializePlatform()
		{
			_secureStorage = AppInfoUtils.IsPackagedApp
				? new PackagedSecureStorageImplementation(Alias)
				: new UnpackagedSecureStorageImplementation(
					UnpackagedAppDataDirectory ?? FileSystem.AppDataDirectory,
					NamespaceUnpackagedStorageByAlias ? Alias : null);
		}

		async Task<string> PlatformGetAsync(string key)
		{
			var encBytes = await _secureStorage.GetAsync(key);

			if (encBytes == null)
				return null;

			var provider = new DataProtectionProvider();

			var buffer = await provider.UnprotectAsync(encBytes.AsBuffer());

			return Encoding.UTF8.GetString(buffer.ToArray());
		}

		async Task PlatformSetAsync(string key, string data)
		{
			var writePath = _secureStorage.CaptureWritePath();
			var bytes = Encoding.UTF8.GetBytes(data);

			// LOCAL=user and LOCAL=machine do not require enterprise auth capability
			var provider = new DataProtectionProvider("LOCAL=user");

			var buffer = await provider.ProtectAsync(bytes.AsBuffer());

			var encBytes = buffer.ToArray();

			await _secureStorage.SetAsync(key, encBytes, writePath);
		}

		bool PlatformRemove(string key) =>
			_secureStorage.Remove(key);

		void PlatformRemoveAll() =>
			_secureStorage.RemoveAll();
	}

	interface ISecureStorageImplementation
	{
		// Returns the file path a write should target, or null for packaged apps, which persist via
		// ApplicationData settings and have no path. Callers must treat the result as optional: the
		// packaged SetAsync ignores writePath, while the unpackaged implementation always returns a
		// non-null path and rejects a null one.
		string CaptureWritePath();

		Task<byte[]> GetAsync(string key);

		Task SetAsync(string key, byte[] value, string writePath);

		bool Remove(string key);

		void RemoveAll();
	}

	class PackagedSecureStorageImplementation : ISecureStorageImplementation
	{
		readonly string _alias;

		public PackagedSecureStorageImplementation(string alias)
		{
			_alias = alias;
		}

		// Packaged apps persist via ApplicationData settings, so there is no file path to capture.
		public string CaptureWritePath() => null;

		public Task<byte[]> GetAsync(string key)
		{
			var settings = GetSettings(_alias);
			var encBytes = settings.Values[key] as byte[];
			return Task.FromResult(encBytes);
		}

		public Task SetAsync(string key, byte[] data, string writePath)
		{
			var settings = GetSettings(_alias);
			settings.Values[key] = data;
			return Task.CompletedTask;
		}

		public bool Remove(string key)
		{
			var settings = GetSettings(_alias);
			return settings.Values.Remove(key);
		}

		public void RemoveAll()
		{
			var settings = GetSettings(_alias);
			settings.Values.Clear();
		}

		static ApplicationDataContainer GetSettings(string name)
		{
			var localSettings = ApplicationData.Current.LocalSettings;
			if (!localSettings.Containers.ContainsKey(name))
				localSettings.CreateContainer(name, ApplicationDataCreateDisposition.Always);
			return localSettings.Containers[name];
		}
	}

	class UnpackagedSecureStorageImplementation : ISecureStorageImplementation
	{
		static readonly object Sync = new();
		static SecureStorageDictionary _secureStorage;
		static string _secureStoragePath;
		readonly string _path;
		readonly string _legacyPath;

		public UnpackagedSecureStorageImplementation()
			: this(FileSystem.AppDataDirectory, alias: null)
		{
		}

		internal UnpackagedSecureStorageImplementation(
			string appDataDirectory,
			string alias = null)
		{
			if (appDataDirectory is null)
				throw new ArgumentNullException(nameof(appDataDirectory));

			var settingsDirectory = Path.Combine(appDataDirectory, "..", "Settings");
			_legacyPath = Path.Combine(settingsDirectory, "securestorage.dat");
			_path = alias is null
				? _legacyPath
				: Path.Combine(settingsDirectory, GetAliasPathSegment(alias), "securestorage.dat");
		}

		public string CaptureWritePath() => _path;

		static string GetAliasPathSegment(string alias) =>
			Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(alias)));

		// Caller must hold Sync. A failed Load leaves the fields unchanged so a later call can retry.
		SecureStorageDictionary GetSecureStorage(out string path)
		{
			path = _path;
			return GetSecureStorage(
				path,
				string.Equals(path, _legacyPath, StringComparison.OrdinalIgnoreCase)
					? null
					: _legacyPath);
		}

		// Caller must hold Sync. Unpackaged FileSystem paths already include publisher/package
		// identity, so the captured path is also the storage namespace for this operation.
		static SecureStorageDictionary GetSecureStorage(
			string path,
			string fallbackPath = null)
		{
			if (_secureStorage is null ||
				!string.Equals(_secureStoragePath, path, StringComparison.OrdinalIgnoreCase))
			{
				var pathExists = File.Exists(path);
				var migrateFallback =
					!pathExists &&
					fallbackPath is not null &&
					File.Exists(fallbackPath);
				// Copy legacy shared data forward on first alias-scoped access. Keep the legacy
				// file intact so unbridged/default callers preserve their historical store.
				var secureStorage = Load(migrateFallback ? fallbackPath : path);
				if (migrateFallback)
					Save(path, secureStorage);

				_secureStorage = secureStorage;
				_secureStoragePath = path;
			}

			return _secureStorage;
		}

		static SecureStorageDictionary Load(string path)
		{
			var secureStorage = new SecureStorageDictionary();
			if (!File.Exists(path))
				return secureStorage;

			try
			{
				using var stream = File.OpenRead(path);

				SecureStorageDictionary readPreferences = JsonSerializer.Deserialize(stream, SecureStorageJsonSerializerContext.Default.SecureStorageDictionary);

				if (readPreferences != null)
				{
					foreach (var pair in readPreferences)
						secureStorage.TryAdd(pair.Key, pair.Value);
				}
			}
			catch (JsonException)
			{
				// if deserialization fails proceed with empty settings
			}

			return secureStorage;
		}

		static void Save(string path, SecureStorageDictionary secureStorage)
		{
			var dir = Path.GetDirectoryName(path);
			Directory.CreateDirectory(dir);

			var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
			try
			{
				using (var stream = File.Create(temporaryPath))
					JsonSerializer.Serialize(stream, secureStorage, SecureStorageJsonSerializerContext.Default.SecureStorageDictionary);

				File.Move(temporaryPath, path, overwrite: true);
			}
			finally
			{
				if (File.Exists(temporaryPath))
					File.Delete(temporaryPath);
			}
		}

		public Task<byte[]> GetAsync(string key)
		{
			lock (Sync)
			{
				var secureStorage = GetSecureStorage(out _);
				secureStorage.TryGetValue(key, out var value);
				return Task.FromResult(value);
			}
		}

		public Task SetAsync(string key, byte[] value) =>
			SetAsync(key, value, CaptureWritePath());

		public Task SetAsync(string key, byte[] value, string writePath)
		{
			if (writePath is null)
				throw new ArgumentNullException(nameof(writePath));

			lock (Sync)
			{
				var secureStorage = GetSecureStorage(
					writePath,
					string.Equals(writePath, _path, StringComparison.OrdinalIgnoreCase)
						? _legacyPath
						: null);
				if (value is null)
					secureStorage.TryRemove(key, out _);
				else
					secureStorage[key] = value;
				Save(writePath, secureStorage);
				return Task.CompletedTask;
			}
		}

		public bool Remove(string key)
		{
			lock (Sync)
			{
				var secureStorage = GetSecureStorage(out var path);
				var result = secureStorage.TryRemove(key, out _);
				Save(path, secureStorage);
				return result;
			}
		}

		public void RemoveAll()
		{
			lock (Sync)
			{
				var secureStorage = GetSecureStorage(out var path);
				secureStorage.Clear();
				Save(path, secureStorage);
			}
		}
	}
}

[JsonSerializable(typeof(SecureStorageDictionary), TypeInfoPropertyName = nameof(SecureStorageDictionary))]
internal partial class SecureStorageJsonSerializerContext : JsonSerializerContext
{
}
