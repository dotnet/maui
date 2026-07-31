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
			var bytes = Encoding.UTF8.GetBytes(data);

			// LOCAL=user and LOCAL=machine do not require enterprise auth capability
			var provider = new DataProtectionProvider("LOCAL=user");

			var buffer = await provider.ProtectAsync(bytes.AsBuffer());

			var encBytes = buffer.ToArray();

			await _secureStorage.SetAsync(key, encBytes);
		}

		bool PlatformRemove(string key) =>
			_secureStorage.Remove(key);

		void PlatformRemoveAll() =>
			_secureStorage.RemoveAll();
	}

	interface ISecureStorageImplementation
	{
		Task<byte[]> GetAsync(string key);

		Task SetAsync(string key, byte[] value);

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

		public Task<byte[]> GetAsync(string key)
		{
			var settings = GetSettings(_alias);
			var encBytes = settings.Values[key] as byte[];
			return Task.FromResult(encBytes);
		}

		public Task SetAsync(string key, byte[] data)
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
		static readonly string AppSecureStoragePath = Path.Combine(FileSystem.AppDataDirectory, "..", "Settings", "securestorage.dat");

		readonly SecureStorageDictionary _secureStorage = new();

		public UnpackagedSecureStorageImplementation()
		{
			Load();
		}

		void Load()
		{
			if (!File.Exists(AppSecureStoragePath))
				return;

			try
			{
				using var stream = File.OpenRead(AppSecureStoragePath);

				SecureStorageDictionary readPreferences = JsonSerializer.Deserialize(stream, SecureStorageJsonSerializerContext.Default.SecureStorageDictionary);

				if (readPreferences != null)
				{
					_secureStorage.Clear();
					foreach (var pair in readPreferences)
						_secureStorage.TryAdd(pair.Key, pair.Value);
				}
			}
			catch (JsonException)
			{
				// if deserialization fails proceed with empty settings
			}
		}

		void Save()
		{
			var dir = Path.GetDirectoryName(AppSecureStoragePath);
			Directory.CreateDirectory(dir);

			using var stream = File.Create(AppSecureStoragePath);
			JsonSerializer.Serialize(stream, _secureStorage, SecureStorageJsonSerializerContext.Default.SecureStorageDictionary);
		}

		public Task<byte[]> GetAsync(string key)
		{
			_secureStorage.TryGetValue(key, out var value);
			return Task.FromResult(value);
		}

		public Task SetAsync(string key, byte[] value)
		{
			if (value is null)
				_secureStorage.TryRemove(key, out _);
			else
				_secureStorage[key] = value;
			Save();
			return Task.CompletedTask;
		}

		public bool Remove(string key)
		{
			var result = _secureStorage.TryRemove(key, out _);
			Save();
			return result;
		}

		public void RemoveAll()
		{
			_secureStorage.Clear();
			Save();
		}
	}
}

[JsonSerializable(typeof(SecureStorageDictionary), TypeInfoPropertyName = nameof(SecureStorageDictionary))]
internal partial class SecureStorageJsonSerializerContext : JsonSerializerContext
{
}
