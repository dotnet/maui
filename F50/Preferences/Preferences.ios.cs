using Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.F50
{
    public partial class Preferences
    {
		public bool ContainsKey(string key)
		{
			var userDefaults = GetUserDefaults();

			var setting = userDefaults[key];
			return setting != null;
		}

		public bool Remove(string key)
		{
			var userDefaults = GetUserDefaults();
			try
			{
				if (userDefaults[key] != null)
				{
					userDefaults.RemoveObject(key);
					userDefaults.Synchronize();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to remove: " + key, " Message: " + ex.Message);
				return false;
			}

			return true;
		}

		public bool Clear()
		{
			var userDefaults = GetUserDefaults();
			try
			{
				var items = userDefaults.ToDictionary();

				foreach (var item in items.Keys)
				{
					if (item is NSString nsString)
						userDefaults.RemoveObject(nsString);
				}
				userDefaults.Synchronize();

			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to clear items, Message: " + ex.Message);
				return false;
			}

			return true;
		}
		
		bool Set<T>(string key, T value)
		{
			var userDefaults = GetUserDefaults();
			
			switch (value)
			{
				case string s:
					userDefaults.SetString(s, key);
					break;
				case int i:
					userDefaults.SetInt(i, key);
					break;
				case bool b:
					userDefaults.SetBool(b, key);
					break;
				case long l:
					userDefaults.SetString(l.ToString(), key);
					break;
				case double d:
					userDefaults.SetDouble(d, key);
					break;
				case float f:
					userDefaults.SetFloat(f, key);
					break;
			}

			try
			{
				userDefaults.Synchronize();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to save: " + key, " Message: " + ex.Message);
				return false;
			}

			return true;
		}

		T Get<T>(string key, T defaultValue)
		{
			var userDefaults = GetUserDefaults();
			if (userDefaults[key] == null)
				return defaultValue;

			object value = null;

			switch (defaultValue)
			{
				case string s:
					value = userDefaults.StringForKey(key);
					break;
				case int i:
					value = userDefaults.IntForKey(key);
					break;
				case bool b:
					value = userDefaults.BoolForKey(key);
					break;
				case long l:
					long lv;
					var lnStr = userDefaults.StringForKey(key);
					if (long.TryParse(lnStr, out lv))
						value = lv;
					break;
				case double d:
					value = userDefaults.DoubleForKey(key);
					break;
				case float f:
					value = userDefaults.FloatForKey(key);
					break;
			}

			return (T)value;
		}

		NSUserDefaults GetUserDefaults() =>
			string.IsNullOrWhiteSpace(SharedName) ?
			NSUserDefaults.StandardUserDefaults :
			new NSUserDefaults(SharedName, NSUserDefaultsType.SuiteName);
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
