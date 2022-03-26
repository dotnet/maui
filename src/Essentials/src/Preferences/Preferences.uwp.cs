using Windows.Storage;

namespace Microsoft.Maui.Storage
{
	class PreferencesImplementation : IPreferences
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
			lock (locker)
			{
				var appDataContainer = GetApplicationDataContainer(sharedName);

				if (value == null)
				{
					if (appDataContainer.Values.ContainsKey(key))
						appDataContainer.Values.Remove(key);
					return;
				}

				appDataContainer.Values[key] = value;
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
						return (T)tempValue;
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
}
