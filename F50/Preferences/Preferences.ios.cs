using Foundation;
using System;
using System.Globalization;

namespace Xamarin.F50
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

		public bool Remove(string key)
		{
			lock (locker)
			{
				if (UserDefaults[key] != null)
					UserDefaults.RemoveObject(key);
			}
			
			return true;
		}

		public bool Clear()
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

			return true;
		}
		
		bool Set<T>(string key, T value)
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
						UserDefaults.SetString(Convert.ToString(l, NumberFormatInfo.InvariantInfo), key);
						break;
					case double d:
						UserDefaults.SetDouble(d, key);
						break;
					case float f:
						UserDefaults.SetFloat(f, key);
						break;
				}
			}

			return true;
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
					case string s:
						value = UserDefaults.StringForKey(key);
						break;
					case int i:
						value = UserDefaults.IntForKey(key);
						break;
					case bool b:
						value = UserDefaults.BoolForKey(key);
						break;
					case long l:
						value = Convert.ToInt64(UserDefaults.StringForKey(key), NumberFormatInfo.InvariantInfo);
						break;
					case double d:
						value = UserDefaults.DoubleForKey(key);
						break;
					case float f:
						value = UserDefaults.FloatForKey(key);
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

		bool disposedValue = false;

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing && userDefaults != null)
				{
					userDefaults.Synchronize();
					userDefaults.Dispose();
					userDefaults = null;
				}
				
				disposedValue = true;
			}
		}
	}
}
