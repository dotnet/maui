using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;

namespace Xamarin.F50
{
    public partial class Preferences
    {
		static readonly object locker = new object();

		public bool ContainsKey(string key)
		{
			lock (locker)
			{
				return Settings.Values.ContainsKey(key);
			}
		}

		public bool Remove(string key)
		{
			lock (locker)
			{
				if (Settings.Values.ContainsKey(key))
				{
					Settings.Values.Remove(key);
					return true;
				}
			}

			return false;
		}

		public bool Clear()
		{
			lock (locker)
			{
				Settings.Values.Clear();
			}

			return true;
		}
		
		bool Set<T>(string key, T value)
		{
			lock (locker)
			{
				Settings.Values[key] = value;
			}

			return true;
		}

		T Get<T>(string key, T defaultValue)
		{
			lock (locker)
			{
				if (Settings.Values.ContainsKey(key))
				{
					var tempValue = settings.Values[key];
					if (tempValue != null)
						return (T)tempValue;
				}
			}

			return defaultValue;
		}

		ApplicationDataContainer settings;
		ApplicationDataContainer Settings
		{
			get
			{
				if (settings == null)
				{
					if (string.IsNullOrWhiteSpace(SharedName))
						settings = ApplicationData.Current.LocalSettings;

					if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(SharedName))
						ApplicationData.Current.LocalSettings.CreateContainer(SharedName, ApplicationDataCreateDisposition.Always);
					settings = ApplicationData.Current.LocalSettings.Containers[SharedName];
				}

				return settings;
			}
		}

		bool disposedValue = false;

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing) { }

				disposedValue = true;
			}
		}
	}
}
