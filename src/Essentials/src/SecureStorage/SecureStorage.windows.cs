using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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
#nullable enable annotations
		string? CaptureWritePath();
#nullable restore

		Task<byte[]> GetAsync(string key);

#nullable enable annotations
		Task SetAsync(string key, byte[] value, string? writePath);
#nullable restore

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
#nullable enable annotations
		public string? CaptureWritePath() => null;
#nullable restore

		public Task<byte[]> GetAsync(string key)
		{
			var settings = GetSettings(_alias);
			var encBytes = settings.Values[key] as byte[];
			return Task.FromResult(encBytes);
		}

#nullable enable annotations
		public Task SetAsync(string key, byte[] data, string? writePath)
		{
			var settings = GetSettings(_alias);
			settings.Values[key] = data;
			return Task.CompletedTask;
		}
#nullable restore

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

		// Caller must hold Sync and the process lock for path.
		SecureStorageDictionary GetSecureStorage(out string path)
		{
			path = _path;
			return GetSecureStorage(
				path,
				string.Equals(path, _legacyPath, StringComparison.OrdinalIgnoreCase)
					? null
					: _legacyPath);
		}

		// Caller must hold Sync and the process lock for path. Reload on every operation so a
		// process never writes a stale snapshot after another process updates the file.
		static SecureStorageDictionary GetSecureStorage(
			string path,
			string fallbackPath = null)
		{
			// Copy legacy shared data forward on first alias-scoped access. Keep the legacy
			// file intact so unbridged/default callers preserve their historical store.
			if (!File.Exists(path) && fallbackPath is not null)
			{
				using var fallbackProcessLock = AcquireProcessLock(fallbackPath);
				if (File.Exists(fallbackPath))
				{
					var migratedStorage = Load(fallbackPath);
					Save(path, migratedStorage);
					return migratedStorage;
				}
			}

			return Load(path);
		}

		internal static IDisposable AcquireProcessLock(string path)
		{
			var normalizedPath = Path.GetFullPath(path).ToUpperInvariant();
			var mutexName =
				$@"Local\Microsoft.Maui.SecureStorage.{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedPath)))}";
			var mutex = new Mutex(initiallyOwned: false, mutexName);
			try
			{
				try
				{
					mutex.WaitOne();
				}
				catch (AbandonedMutexException)
				{
					// Ownership is granted when an abandoned mutex is observed.
				}

				return new ProcessLock(mutex);
			}
			catch
			{
				mutex.Dispose();
				throw;
			}
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
				using var processLock = AcquireProcessLock(_path);
				var secureStorage = GetSecureStorage(out _);
				secureStorage.TryGetValue(key, out var value);
				return Task.FromResult(value);
			}
		}

		public Task SetAsync(string key, byte[] value) =>
			SetAsync(key, value, CaptureWritePath());

#nullable enable annotations
		public Task SetAsync(string key, byte[] value, string? writePath)
		{
			if (writePath is null)
				throw new ArgumentNullException(nameof(writePath));

			lock (Sync)
			{
				using var processLock = AcquireProcessLock(writePath);
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
#nullable restore

		public bool Remove(string key)
		{
			lock (Sync)
			{
				using var processLock = AcquireProcessLock(_path);
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
				using var processLock = AcquireProcessLock(_path);
				var secureStorage = GetSecureStorage(out var path);
				secureStorage.Clear();
				Save(path, secureStorage);
			}
		}

		sealed class ProcessLock : IDisposable
		{
			readonly Mutex _mutex;

			public ProcessLock(Mutex mutex)
			{
				_mutex = mutex;
			}

			public void Dispose()
			{
				try
				{
					_mutex.ReleaseMutex();
				}
				finally
				{
					_mutex.Dispose();
				}
			}
		}
	}
}

[JsonSerializable(typeof(SecureStorageDictionary), TypeInfoPropertyName = nameof(SecureStorageDictionary))]
internal partial class SecureStorageJsonSerializerContext : JsonSerializerContext
{
}
