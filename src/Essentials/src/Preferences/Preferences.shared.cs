using System;

namespace Microsoft.Maui.Essentials
{
	public static partial class Preferences
	{
		internal static string GetPrivatePreferencesSharedName(string feature) =>
			$"{AppInfo.PackageName}.xamarinessentials.{feature}";

		// overloads

		public static bool ContainsKey(string key) =>
			ContainsKey(key, null);

		public static void Remove(string key) =>
			Remove(key, null);

		public static void Clear() =>
			Clear(null);

		public static string Get(string key, string defaultValue) =>
			Get(key, defaultValue, null);

		public static bool Get(string key, bool defaultValue) =>
			Get(key, defaultValue, null);

		public static int Get(string key, int defaultValue) =>
			Get(key, defaultValue, null);

		public static double Get(string key, double defaultValue) =>
			Get(key, defaultValue, null);

		public static float Get(string key, float defaultValue) =>
			Get(key, defaultValue, null);

		public static long Get(string key, long defaultValue) =>
			Get(key, defaultValue, null);

		public static void Set(string key, string value) =>
			Set(key, value, null);

		public static void Set(string key, bool value) =>
			Set(key, value, null);

		public static void Set(string key, int value) =>
			Set(key, value, null);

		public static void Set(string key, double value) =>
			Set(key, value, null);

		public static void Set(string key, float value) =>
			Set(key, value, null);

		public static void Set(string key, long value) =>
			Set(key, value, null);

		// shared -> platform

		public static bool ContainsKey(string key, string sharedName) =>
			PlatformContainsKey(key, sharedName);

		public static void Remove(string key, string sharedName) =>
			PlatformRemove(key, sharedName);

		public static void Clear(string sharedName) =>
			PlatformClear(sharedName);

		public static string Get(string key, string defaultValue, string sharedName) =>
			PlatformGet<string>(key, defaultValue, sharedName);

		public static bool Get(string key, bool defaultValue, string sharedName) =>
			PlatformGet<bool>(key, defaultValue, sharedName);

		public static int Get(string key, int defaultValue, string sharedName) =>
			PlatformGet<int>(key, defaultValue, sharedName);

		public static double Get(string key, double defaultValue, string sharedName) =>
			PlatformGet<double>(key, defaultValue, sharedName);

		public static float Get(string key, float defaultValue, string sharedName) =>
			PlatformGet<float>(key, defaultValue, sharedName);

		public static long Get(string key, long defaultValue, string sharedName) =>
			PlatformGet<long>(key, defaultValue, sharedName);

		public static void Set(string key, string value, string sharedName) =>
			PlatformSet<string>(key, value, sharedName);

		public static void Set(string key, bool value, string sharedName) =>
			PlatformSet<bool>(key, value, sharedName);

		public static void Set(string key, int value, string sharedName) =>
			PlatformSet<int>(key, value, sharedName);

		public static void Set(string key, double value, string sharedName) =>
			PlatformSet<double>(key, value, sharedName);

		public static void Set(string key, float value, string sharedName) =>
			PlatformSet<float>(key, value, sharedName);

		public static void Set(string key, long value, string sharedName) =>
			PlatformSet<long>(key, value, sharedName);

		// DateTime

		public static DateTime Get(string key, DateTime defaultValue) =>
			Get(key, defaultValue, null);

		public static void Set(string key, DateTime value) =>
			Set(key, value, null);

		public static DateTime Get(string key, DateTime defaultValue, string sharedName) =>
			DateTime.FromBinary(PlatformGet<long>(key, defaultValue.ToBinary(), sharedName));

		public static void Set(string key, DateTime value, string sharedName) =>
			PlatformSet<long>(key, value.ToBinary(), sharedName);
	}
}
