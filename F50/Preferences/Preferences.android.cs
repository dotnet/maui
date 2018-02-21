using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Preferences;

namespace Xamarin.F50
{
	public partial class Preferences
	{
		static readonly object locker = new object();

		public bool ContainsKey(string key)
		{
			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreferences())
				{
					return sharedPreferences.Contains(key);
				}
			}
		}

		public void Remove(string key)
		{
			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreferences())
				using (var editor = sharedPreferences.Edit())
				{
					editor.Remove(key).Commit();
				}
			}
		}

		public void Clear()
		{
			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreferences())
				using (var editor = sharedPreferences.Edit())
				{
					editor.Clear().Commit();
				}
			}
		}

		void Set<T>(string key, T value)
		{
			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreferences())
				using (var editor = sharedPreferences.Edit())
				{
					switch (value)
					{
						case string s:
							editor.PutString(key, s);
							break;
						case int i:
							editor.PutInt(key, i);
							break;
						case bool b:
							editor.PutBoolean(key, b);
							break;
						case long l:
							editor.PutLong(key, l);
							break;
						case double d:
							editor.PutString(key, Convert.ToString(d, NumberFormatInfo.InvariantInfo));
							break;
						case float f:
							editor.PutFloat(key, f);
							break;
					}
					editor.Apply();
				}
			}
		}

		T Get<T> (string key, T defaultValue)
		{
			lock (locker)
			{
				object value = null;
				using (var sharedPreferences = GetSharedPreferences())
				{
					switch (defaultValue)
					{
						case string s:
							value = sharedPreferences.GetString(key, s);
							break;
						case int i:
							value = sharedPreferences.GetInt(key, i);
							break;
						case bool b:
							value = sharedPreferences.GetBoolean(key, b);
							break;
						case long l:
							value = sharedPreferences.GetLong(key, l);
							break;
						case double d:
							value = Convert.ToDouble(sharedPreferences.GetString(key, null), NumberFormatInfo.InvariantInfo);
							break;
						case float f:
							value = sharedPreferences.GetFloat(key, f);
							break;
					}
				}

				return (T)value;
			}
		}

		ISharedPreferences GetSharedPreferences ()
		{
			var context = Application.Context;
			if (context == null)
				return null;

			return string.IsNullOrWhiteSpace(SharedName) ?
				PreferenceManager.GetDefaultSharedPreferences(context) :
					context.GetSharedPreferences(SharedName, FileCreationMode.Private);
		}
	}
}
