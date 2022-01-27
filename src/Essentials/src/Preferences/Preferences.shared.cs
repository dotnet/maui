using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Preferences']/Docs" />
	public static partial class Preferences
	{
		internal static string GetPrivatePreferencesSharedName(string feature) =>
			$"{AppInfo.PackageName}.microsoft.maui.essentials.{feature}";

		// overloads

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='ContainsKey'][0]/Docs" />
		public static bool ContainsKey(string key) =>
			ContainsKey(key, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Remove'][0]/Docs" />
		public static void Remove(string key) =>
			Remove(key, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Clear'][0]/Docs" />
		public static void Clear() =>
			Clear(null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][6]/Docs" />
		public static string Get(string key, string defaultValue) =>
			Get(key, defaultValue, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][0]/Docs" />
		public static bool Get(string key, bool defaultValue) =>
			Get(key, defaultValue, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][3]/Docs" />
		public static int Get(string key, int defaultValue) =>
			Get(key, defaultValue, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][2]/Docs" />
		public static double Get(string key, double defaultValue) =>
			Get(key, defaultValue, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][5]/Docs" />
		public static float Get(string key, float defaultValue) =>
			Get(key, defaultValue, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][4]/Docs" />
		public static long Get(string key, long defaultValue) =>
			Get(key, defaultValue, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][6]/Docs" />
		public static void Set(string key, string value) =>
			Set(key, value, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][0]/Docs" />
		public static void Set(string key, bool value) =>
			Set(key, value, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][3]/Docs" />
		public static void Set(string key, int value) =>
			Set(key, value, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][2]/Docs" />
		public static void Set(string key, double value) =>
			Set(key, value, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][5]/Docs" />
		public static void Set(string key, float value) =>
			Set(key, value, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][4]/Docs" />
		public static void Set(string key, long value) =>
			Set(key, value, null);

		// shared -> platform

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='ContainsKey'][1]/Docs" />
		public static bool ContainsKey(string key, string sharedName) =>
			PlatformContainsKey(key, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Remove'][1]/Docs" />
		public static void Remove(string key, string sharedName) =>
			PlatformRemove(key, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Clear'][1]/Docs" />
		public static void Clear(string sharedName) =>
			PlatformClear(sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][13]/Docs" />
		public static string Get(string key, string defaultValue, string sharedName) =>
			PlatformGet<string>(key, defaultValue, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][7]/Docs" />
		public static bool Get(string key, bool defaultValue, string sharedName) =>
			PlatformGet<bool>(key, defaultValue, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][10]/Docs" />
		public static int Get(string key, int defaultValue, string sharedName) =>
			PlatformGet<int>(key, defaultValue, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][9]/Docs" />
		public static double Get(string key, double defaultValue, string sharedName) =>
			PlatformGet<double>(key, defaultValue, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][12]/Docs" />
		public static float Get(string key, float defaultValue, string sharedName) =>
			PlatformGet<float>(key, defaultValue, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][11]/Docs" />
		public static long Get(string key, long defaultValue, string sharedName) =>
			PlatformGet<long>(key, defaultValue, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][13]/Docs" />
		public static void Set(string key, string value, string sharedName) =>
			PlatformSet<string>(key, value, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][7]/Docs" />
		public static void Set(string key, bool value, string sharedName) =>
			PlatformSet<bool>(key, value, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][10]/Docs" />
		public static void Set(string key, int value, string sharedName) =>
			PlatformSet<int>(key, value, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][9]/Docs" />
		public static void Set(string key, double value, string sharedName) =>
			PlatformSet<double>(key, value, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][12]/Docs" />
		public static void Set(string key, float value, string sharedName) =>
			PlatformSet<float>(key, value, sharedName);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][11]/Docs" />
		public static void Set(string key, long value, string sharedName) =>
			PlatformSet<long>(key, value, sharedName);

		// DateTime

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][1]/Docs" />
		public static DateTime Get(string key, DateTime defaultValue) =>
			Get(key, defaultValue, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][1]/Docs" />
		public static void Set(string key, DateTime value) =>
			Set(key, value, null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Get'][8]/Docs" />
		public static DateTime Get(string key, DateTime defaultValue, string sharedName) =>
			DateTime.FromBinary(PlatformGet<long>(key, defaultValue.ToBinary(), sharedName));

		/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="//Member[@MemberName='Set'][8]/Docs" />
		public static void Set(string key, DateTime value, string sharedName) =>
			PlatformSet<long>(key, value.ToBinary(), sharedName);
	}
}
