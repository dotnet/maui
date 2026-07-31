using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
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
				: new UnpackagedSecureStorageImplementation();
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

		static string AppSecureStoragePath =>
			Path.Combine(FileSystem.AppDataDirectory, "..", "Settings", "securestorage.dat");

		public string CaptureWritePath() => AppSecureStoragePath;

		// Caller must hold Sync. A failed Load leaves the fields unchanged so a later call can retry.
		static SecureStorageDictionary GetSecureStorage(out string path)
		{
			path = AppSecureStoragePath;
			return GetSecureStorage(path);
		}

		// Caller must hold Sync. Unpackaged FileSystem paths already include publisher/package
		// identity, so the captured path is also the storage namespace for this operation.
		static SecureStorageDictionary GetSecureStorage(string path)
		{
			if (_secureStorage is null ||
				!string.Equals(_secureStoragePath, path, StringComparison.OrdinalIgnoreCase))
			{
				var secureStorage = Load(path);
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

			using var stream = File.Create(path);
			JsonSerializer.Serialize(stream, secureStorage, SecureStorageJsonSerializerContext.Default.SecureStorageDictionary);
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
				var secureStorage = GetSecureStorage(writePath);
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
