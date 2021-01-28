using System.Linq;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class Preferences
    {
        const string separator = "~";

        static readonly object locker = new object();

        static bool PlatformContainsKey(string key, string sharedName)
        {
            lock (locker)
            {
                return Preference.Contains(GetFullKey(key, sharedName));
            }
        }

        static void PlatformRemove(string key, string sharedName)
        {
            lock (locker)
            {
                var fullKey = GetFullKey(key, sharedName);
                if (Preference.Contains(fullKey))
                    Preference.Remove(fullKey);
            }
        }

        static void PlatformClear(string sharedName)
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

        static void PlatformSet<T>(string key, T value, string sharedName)
        {
            lock (locker)
            {
                var fullKey = GetFullKey(key, sharedName);
                if (value == null)
                    Preference.Remove(fullKey);
                else
                    Preference.Set(fullKey, value);
            }
        }

        static T PlatformGet<T>(string key, T defaultValue, string sharedName)
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
