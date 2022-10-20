using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Preferences;

namespace Microsoft.Maui.Storage
{
	class PreferencesImplementation : IPreferences
	{
		readonly object locker = new object();

		public bool ContainsKey(string key, string sharedName)
		{
			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreferences(sharedName))
				{
					return sharedPreferences.Contains(key);
				}
			}
		}

		public void Remove(string key, string sharedName)
		{
			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreferences(sharedName))
				using (var editor = sharedPreferences.Edit())
				{
					editor.Remove(key).Apply();
				}
			}
		}

		public void Clear(string sharedName)
		{
			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreferences(sharedName))
				using (var editor = sharedPreferences.Edit())
				{
					editor.Clear().Apply();
				}
			}
		}

		public void Set<T>(string key, T value, string sharedName)
		{
			Preferences.CheckIsSupportedType<T>();

			lock (locker)
			{
				using (var sharedPreferences = GetSharedPreferences(sharedName))
				using (var editor = sharedPreferences.Edit())
				{
					if (value == null)
					{
						editor.Remove(key);
					}
					else
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
								var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
								editor.PutString(key, valueString);
								break;
							case float f:
								editor.PutFloat(key, f);
								break;
							case DateTime dt:
								editor.PutLong(key, dt.ToBinary());
								break;
						}
					}
					editor.Apply();
				}
			}
		}

		public T Get<T>(string key, T defaultValue, string sharedName)
		{
			lock (locker)
			{
				object value = null;
				using (var sharedPreferences = GetSharedPreferences(sharedName))
				{
					if (defaultValue == null)
					{
						value = sharedPreferences.GetString(key, null);
					}
					else
					{
						switch (defaultValue)
						{
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
								var savedDouble = sharedPreferences.GetString(key, null);
								if (string.IsNullOrWhiteSpace(savedDouble))
								{
									value = defaultValue;
								}
								else
								{
									if (!double.TryParse(savedDouble, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var outDouble))
									{
										var maxString = Convert.ToString(double.MaxValue, CultureInfo.InvariantCulture);
										outDouble = savedDouble.Equals(maxString, StringComparison.Ordinal) ? double.MaxValue : double.MinValue;
									}

									value = outDouble;
								}
								break;
							case float f:
								value = sharedPreferences.GetFloat(key, f);
								break;
							case string s:
								// the case when the string is not null
								value = sharedPreferences.GetString(key, s);
								break;
							case DateTime dt:
								var encodedValue = sharedPreferences.GetLong(key, dt.ToBinary());
								value = DateTime.FromBinary(encodedValue);
								break;
						}
					}
				}

				return (T)value;
			}
		}

		internal static ISharedPreferences GetSharedPreferences(string sharedName)
		{
			var context = Application.Context;

			return string.IsNullOrWhiteSpace(sharedName) ?
#pragma warning disable CS0618 // Type or member is obsolete
				PreferenceManager.GetDefaultSharedPreferences(context) :
#pragma warning restore CS0618 // Type or member is obsolete
					context.GetSharedPreferences(sharedName, FileCreationMode.Private);
		}
	}
}
