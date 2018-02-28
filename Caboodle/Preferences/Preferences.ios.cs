using Foundation;
using System;
using System.Globalization;

namespace Microsoft.Caboodle
{
	public partial class Preferences
	{
		static readonly object locker = new object();

		public bool ContainsKey(string key)
		{
			lock (locker)
			{
				return UserDefaults[key] != null;
			}
		}

		public void Remove(string key)
		{
			lock (locker)
			{
				if (UserDefaults[key] != null)
					UserDefaults.RemoveObject(key);
			}
		}

		public void Clear()
		{
			lock (locker)
			{
				var items = UserDefaults.ToDictionary();

				foreach (var item in items.Keys)
				{
					if (item is NSString nsString)
						UserDefaults.RemoveObject(nsString);
				}
			}
		}

		void Set<T>(string key, T value)
		{
			lock (locker)
			{
				switch (value)
				{
					case string s:
						UserDefaults.SetString(s, key);
						break;
					case int i:
						UserDefaults.SetInt(i, key);
						break;
					case bool b:
						UserDefaults.SetBool(b, key);
						break;
					case long l:
						var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
						UserDefaults.SetString(valueString, key);
						break;
					case double d:
						UserDefaults.SetDouble(d, key);
						break;
					case float f:
						UserDefaults.SetFloat(f, key);
						break;
				}
			}
		}

		T Get<T>(string key, T defaultValue)
		{
			object value = null;

			lock (locker)
			{
				if (UserDefaults[key] == null)
					return defaultValue;

				switch (defaultValue)
				{
					case int i:
						value = (int)(nint)UserDefaults.IntForKey(key);
						break;
					case bool b:
						value = UserDefaults.BoolForKey(key);
						break;
					case long l:
						var savedLong = UserDefaults.StringForKey(key);
						value = Convert.ToInt64(savedLong, CultureInfo.InvariantCulture);
						break;
					case double d:
						value = UserDefaults.DoubleForKey(key);
						break;
					case float f:
						value = UserDefaults.FloatForKey(key);
						break;
					case string s:
						// the case when the string is not null
						value = UserDefaults.StringForKey(key);
						break;
					default:
						// the case when the string is null
						if (typeof(T) == typeof(string))
							value = UserDefaults.StringForKey(key);
						break;
				}
			}

			return (T)value;
		}

		NSUserDefaults userDefaults = null;
		NSUserDefaults UserDefaults
		{
			get
			{
				if (userDefaults == null)
				{
					if (!string.IsNullOrWhiteSpace(SharedName))
						userDefaults = new NSUserDefaults(SharedName, NSUserDefaultsType.SuiteName);
					else
						userDefaults = NSUserDefaults.StandardUserDefaults;
				}

				return userDefaults;
			}
		}
	}
}
