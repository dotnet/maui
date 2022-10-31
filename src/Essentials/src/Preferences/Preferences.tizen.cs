using System;
using System.Linq;
using Tizen.Applications;

namespace Microsoft.Maui.Storage
{
	class PreferencesImplementation : IPreferences
	{
		const string separator = "~";

		static readonly object locker = new object();

		public bool ContainsKey(string key, string sharedName)
		{
			lock (locker)
			{
				return Preference.Contains(GetFullKey(key, sharedName));
			}
		}

		public void Remove(string key, string sharedName)
		{
			lock (locker)
			{
				var fullKey = GetFullKey(key, sharedName);
				if (Preference.Contains(fullKey))
					Preference.Remove(fullKey);
			}
		}

		public void Clear(string sharedName)
		{
			lock (locker)
			{
				if (string.IsNullOrEmpty(sharedName))
				{
					Preference.RemoveAll();
				}
				else
				{
					var keys = Preference.Keys.Where(key => key.StartsWith($"{sharedName}{separator}")).ToList();
					foreach (var key in keys)
						Preference.Remove(key);
				}
			}
		}

		public void Set<T>(string key, T value, string sharedName)
		{
			Preferences.CheckIsSupportedType<T>();

			lock (locker)
			{
				var fullKey = GetFullKey(key, sharedName);
				if (value == null)
					Preference.Remove(fullKey);
				else if (value is DateTime dt)
				{
					Preference.Set(fullKey, dt.ToBinary());
				}
				else
					Preference.Set(fullKey, value);
			}
		}

		public T Get<T>(string key, T defaultValue, string sharedName)
		{
			lock (locker)
			{
				var value = defaultValue;
				var fullKey = GetFullKey(key, sharedName);
				if (Preference.Contains(fullKey))
				{
					switch (defaultValue)
					{
						case int i:
						case bool b:
						case long l:
						case double d:
						case float f:
						case string s:
							value = Preference.Get<T>(fullKey);
							break;
						case DateTime dt:
							var encodedValue = Preference.Get<long>(fullKey);
							value = (T)(object)DateTime.FromBinary(encodedValue);
							break;
						default:
							// the case when the string is null
							if (typeof(T) == typeof(string))
								value = (T)(object)Preference.Get<string>(fullKey);
							break;
					}
				}
				return value;
			}
		}

		static string GetFullKey(string key, string sharedName = null)
		{
			if (string.IsNullOrEmpty(sharedName))
				return key;
			return $"{sharedName}{separator}{key}";
		}
	}
}
