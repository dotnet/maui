#nullable enable

namespace Microsoft.Maui.Storage
{
	public interface IPreferences
	{
		bool ContainsKey(string key, string? sharedName = null);

		void Remove(string key, string? sharedName = null);

		void Clear(string? sharedName = null);

		void Set<T>(string key, T value, string? sharedName = null);

		T Get<T>(string key, T defaultValue, string? sharedName = null);
	}

	public static class Preferences
	{
		internal static string GetPrivatePreferencesSharedName(string feature) =>
			$"{ApplicationModel.AppInfo.Current.PackageName}.microsoft.maui.essentials.{feature}";

		static IPreferences? defaultImplementation;

		public static IPreferences Default =>
			defaultImplementation ??= new PreferencesImplementation();

		internal static void SetDefault(IPreferences? implementation) =>
			defaultImplementation = implementation;
	}
}
