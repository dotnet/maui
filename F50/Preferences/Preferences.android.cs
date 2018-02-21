using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Preferences;

namespace Xamarin.F50
{
	public partial class Preferences
	{
		public bool ContainsKey(string key)
		{
			using (var sharedPreferences = GetSharedPreferences())
			{
				if (sharedPreferences == null)
					return false;

				return sharedPreferences.Contains(key);
			}
		}

		public bool Remove(string key)
		{
			using (var sharedPreferences = GetSharedPreferences())
			{
				if (sharedPreferences == null)
					return false;

				return sharedPreferences.Edit().Remove(key).Commit();
			}
		}

		public bool Clear()
		{
			using (var sharedPreferences = GetSharedPreferences())
			{
				if (sharedPreferences == null)
					return false;

				using (var editor = sharedPreferences.Edit())
				{
					return editor.Clear().Commit();
				}
			}
		}

		bool Set<T>(string key, T value)
		{
			using (var sharedPreferences = GetSharedPreferences())
			{
				if (sharedPreferences == null)
					return false;

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
							var strDbl = Convert.ToString(d, CultureInfo.InvariantCulture);
							editor.PutString(key, strDbl);
							break;
						case float f:
							editor.PutFloat(key, f);
							break;
					}
					return editor.Commit();
				}
			}
		}

		T Get<T> (string key, T defaultValue)
		{
			object value = null;
			using (var sharedPreferences = GetSharedPreferences())
			{
				if (sharedPreferences == null)
					return default;

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
						double dbl;
						if (double.TryParse(sharedPreferences.GetString(key, null), out dbl))
							value = dbl;
						break;
					case float f:
						value = sharedPreferences.GetFloat(key, f);
						break;
				}
			}

			return (T)value;
		}

		ISharedPreferences GetSharedPreferences ()
		{
			var context = Platform.CurrentContext;
			if (context == null)
				return null;

			return string.IsNullOrWhiteSpace(SharedName) ?
				PreferenceManager.GetDefaultSharedPreferences(Application.Context) :
					Application.Context.GetSharedPreferences(SharedName, FileCreationMode.Private);
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
