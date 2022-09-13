using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel;
using Windows.Storage;

using PreferencesDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, string>>;
using ShareNameDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, string>;

namespace Microsoft.Maui.Storage
{
	class PreferencesImplementation : IPreferences
	{
		readonly IPreferences _preferences;

		public PreferencesImplementation()
		{
			_preferences = AppInfoUtils.IsPackagedApp
				? new PackagedPreferencesImplementation()
				: new UnpackagedPreferencesImplementation();
		}

		public bool ContainsKey(string key, string sharedName) =>
			_preferences.ContainsKey(key, sharedName);

		public void Remove(string key, string sharedName) =>
			_preferences.Remove(key, sharedName);

		public void Clear(string sharedName) =>
			_preferences.Clear(sharedName);

		public void Set<T>(string key, T value, string sharedName) =>
			_preferences.Set<T>(key, value, sharedName);

		public T Get<T>(string key, T defaultValue, string sharedName) =>
			_preferences.Get<T>(key, defaultValue, sharedName);
	}

	class PackagedPreferencesImplementation : IPreferences
	{
		static readonly object locker = new object();

		public bool ContainsKey(string key, string sharedName)
		{
			lock (locker)
			{
				var appDataContainer = GetApplicationDataContainer(sharedName);
				return appDataContainer.Values.ContainsKey(key);
			}
		}

		public void Remove(string key, string sharedName)
		{
			lock (locker)
			{
				var appDataContainer = GetApplicationDataContainer(sharedName);
				if (appDataContainer.Values.ContainsKey(key))
					appDataContainer.Values.Remove(key);
			}
		}

		public void Clear(string sharedName)
		{
			lock (locker)
			{
				var appDataContainer = GetApplicationDataContainer(sharedName);
				appDataContainer.Values.Clear();
			}
		}

		public void Set<T>(string key, T value, string sharedName)
		{
			Preferences.CheckIsSupportedType<T>();

			lock (locker)
			{
				var appDataContainer = GetApplicationDataContainer(sharedName);

				if (value == null)
				{
					if (appDataContainer.Values.ContainsKey(key))
						appDataContainer.Values.Remove(key);
					return;
				}

				if (value is DateTime dt)
				{
					appDataContainer.Values[key] = dt.ToBinary();
				}
				else
				{
					appDataContainer.Values[key] = value;
				}
			}
		}

		public T Get<T>(string key, T defaultValue, string sharedName)
		{
			lock (locker)
			{
				var appDataContainer = GetApplicationDataContainer(sharedName);
				if (appDataContainer.Values.ContainsKey(key))
				{
					var tempValue = appDataContainer.Values[key];
					if (tempValue != null)
					{
						if (defaultValue is DateTime dt)
						{
							return (T)(object)DateTime.FromBinary((long)tempValue);
						}
						else
						{
							return (T)tempValue;
						}
					}
				}
			}

			return defaultValue;
		}

		static ApplicationDataContainer GetApplicationDataContainer(string sharedName)
		{
			var localSettings = ApplicationData.Current.LocalSettings;
			if (string.IsNullOrWhiteSpace(sharedName))
				return localSettings;

			if (!localSettings.Containers.ContainsKey(sharedName))
				localSettings.CreateContainer(sharedName, ApplicationDataCreateDisposition.Always);

			return localSettings.Containers[sharedName];
		}
	}

	class UnpackagedPreferencesImplementation : IPreferences
	{
		static readonly string AppPreferencesPath = Path.Combine(FileSystem.AppDataDirectory, "..", "Settings", "preferences.dat");

		readonly PreferencesDictionary _preferences = new();

		public UnpackagedPreferencesImplementation()
		{
			Load();

			_preferences.GetOrAdd(string.Empty, _ => new ShareNameDictionary());
		}

		public bool ContainsKey(string key, string sharedName = null)
		{
			if (_preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
			{
				return inner.ContainsKey(key);
			}

			return false;
		}

		public void Remove(string key, string sharedName = null)
		{
			if (_preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
			{
				inner.TryRemove(key, out _);
				Save();
			}
		}

		public void Clear(string sharedName = null)
		{
			if (_preferences.TryGetValue(CleanSharedName(sharedName), out var prefs))
			{
				prefs.Clear();
				Save();
			}
		}

		public void Set<T>(string key, T value, string sharedName = null)
		{
			var prefs = _preferences.GetOrAdd(CleanSharedName(sharedName), _ => new ShareNameDictionary());

			if (value is null)
				prefs.TryRemove(key, out _);
			else
				prefs[key] = string.Format(CultureInfo.InvariantCulture, "{0}", value);

			Save();
		}

		public T Get<T>(string key, T defaultValue, string sharedName = null)
		{
			if (_preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
			{
				if (inner.TryGetValue(key, out var value) && value is not null)
				{
					try
					{
						return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
					}
					catch (FormatException)
					{
						// bad get, fall back to default
					}
				}
			}

			return defaultValue;
		}

		void Load()
		{
			if (!File.Exists(AppPreferencesPath))
				return;

			try
			{
				using var stream = File.OpenRead(AppPreferencesPath);

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
				var readPreferences = JsonSerializer.Deserialize<PreferencesDictionary>(stream);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

				if (readPreferences != null)
				{
					_preferences.Clear();
					foreach (var pair in readPreferences)
						_preferences.TryAdd(pair.Key, pair.Value);
				}
			}
			catch (JsonException)
			{
				// if deserialization fails proceed with empty settings
			}
		}

		void Save()
		{
			var dir = Path.GetDirectoryName(AppPreferencesPath);
			Directory.CreateDirectory(dir);

			using var stream = File.Create(AppPreferencesPath);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
			JsonSerializer.Serialize(stream, _preferences);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
		}

		static string CleanSharedName(string sharedName) =>
			string.IsNullOrEmpty(sharedName) ? string.Empty : sharedName;
	}
}
