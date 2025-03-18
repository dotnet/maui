#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.Maui.Storage
{
	/// <summary>
	/// The Preferences API helps to store application preferences in a key/value store.
	/// </summary>
	/// <remarks>
	/// <para>Each platform uses the platform-provided APIs for storing application/user preferences:</para>
	/// <list type="bullet">
	/// <item><description>iOS: NSUserDefaults</description></item>
	/// <item><description>Android: SharedPreferences</description></item>
	/// <item><description>Windows: ApplicationDataContainer</description></item>
	/// </list>
	/// </remarks>
	public interface IPreferences
	{
		/// <summary>
		/// Checks for the existence of a given key.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="sharedName">Shared container name.</param>
		/// <returns><see langword="true"/> if the key exists in the preferences, otherwise <see langword="false"/>.</returns>
		bool ContainsKey(string key, string? sharedName = null);

		/// <summary>
		/// Removes a key and its associated value if it exists.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <param name="sharedName">Shared container name.</param>
		void Remove(string key, string? sharedName = null);

		/// <summary>
		/// Clears all keys and values.
		/// </summary>
		/// <param name="sharedName">Shared container name.</param>
		void Clear(string? sharedName = null);

		/// <summary>
		/// Sets a value for a given key.
		/// </summary>
		/// <typeparam name="T">Type of the object that is stored in this preference.</typeparam>
		/// <param name="key">The key to set the value for.</param>
		/// <param name="value">Value to set.</param>
		/// <param name="sharedName">Shared container name.</param>
		void Set<T>(string key, T value, string? sharedName = null);

		/// <summary>
		/// Gets the value for a given key, or the default specified if the key does not exist.
		/// </summary>
		/// <typeparam name="T">The type of the object stored for this preference.</typeparam>
		/// <param name="key">The key to retrieve the value for.</param>
		/// <param name="defaultValue">The default value to return when no existing value for <paramref name="key"/> exists.</param>
		/// <param name="sharedName">Shared container name.</param>
		/// <returns>Value for the given key, or the value in <paramref name="defaultValue"/> if it does not exist.</returns>
		T Get<T>(string key, T defaultValue, string? sharedName = null);
	}

	/// <summary>
	/// The Preferences API helps to store application preferences in a key/value store.
	/// </summary>
	/// <remarks>
	/// <para>Each platform uses the platform provided native APIs for storing application/user preferences:</para>
	/// <list type="bullet">
	/// <item>iOS: NSUserDefaults</item>
	/// <item>Android: SharedPreferences</item>
	/// <item>Windows: ApplicationDataContainer</item>
	/// </list>
	/// </remarks>
	public static class Preferences
	{
		// overloads

		/// <summary>
		/// Checks the existence of a given key.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns><see langword="true"/> if the key exists in the preferences, otherwise <see langword="false"/>.</returns>
		public static bool ContainsKey(string key) =>
			ContainsKey(key, null);

		/// <summary>
		/// Removes a key and its associated value if it exists.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		public static void Remove(string key) =>
			Remove(key, null);

		/// <summary>
		/// Clears all keys and values.
		/// </summary>
		public static void Clear() =>
			Clear(null);

		/// <summary>
		/// Gets the value for a given key, or the default specified if the key does not exist.
		/// </summary>
		/// <param name="key">The key to retrieve the value for.</param>
		/// <param name="defaultValue">The default value to return when no existing value for <paramref name="key"/> exists.</param>
		/// <returns>Value for the given key, or the value in <paramref name="defaultValue"/> if it does not exist.</returns>
#if !NETSTANDARD
		[return: NotNullIfNotNull("defaultValue")]
#endif
		public static string? Get(string key, string? defaultValue) =>
			Get(key, defaultValue, null);

		/// <inheritdoc cref="Get(string, string?)"/>
		public static bool Get(string key, bool defaultValue) =>
			Get(key, defaultValue, null);

		/// <inheritdoc cref="Get(string, string?)"/>
		public static int Get(string key, int defaultValue) =>
			Get(key, defaultValue, null);

		/// <inheritdoc cref="Get(string, string?)"/>
		public static double Get(string key, double defaultValue) =>
			Get(key, defaultValue, null);

		/// <inheritdoc cref="Get(string, string?)"/>
		public static float Get(string key, float defaultValue) =>
			Get(key, defaultValue, null);

		/// <inheritdoc cref="Get(string, string?)"/>
		public static long Get(string key, long defaultValue) =>
			Get(key, defaultValue, null);

		/// <summary>
		/// Sets a value for a given key.
		/// </summary>
		/// <param name="key">The key to set the value for.</param>
		/// <param name="value">Value to set.</param>
		public static void Set(string key, string? value) =>
			Set(key, value, null);

		/// <inheritdoc cref="Set(string, string?)"/>
		public static void Set(string key, bool value) =>
			Set(key, value, null);

		/// <inheritdoc cref="Set(string, string?)"/>
		public static void Set(string key, int value) =>
			Set(key, value, null);

		/// <inheritdoc cref="Set(string, string?)"/>
		public static void Set(string key, double value) =>
			Set(key, value, null);

		/// <inheritdoc cref="Set(string, string?)"/>
		public static void Set(string key, float value) =>
			Set(key, value, null);

		/// <inheritdoc cref="Set(string, string?)"/>
		public static void Set(string key, long value) =>
			Set(key, value, null);

		// shared -> platform

		/// <inheritdoc cref="IPreferences.ContainsKey(string, string?)"/>
		public static bool ContainsKey(string key, string? sharedName) =>
			Current.ContainsKey(key, sharedName);

		/// <inheritdoc cref="IPreferences.Remove(string, string?)"/>
		public static void Remove(string key, string? sharedName) =>
			Current.Remove(key, sharedName);

		/// <inheritdoc cref="IPreferences.Clear(string?)"/>
		public static void Clear(string? sharedName) =>
			Current.Clear(sharedName);

		/// <inheritdoc cref="IPreferences.Get{T}(string, T, string?)"/>
#if !NETSTANDARD
		[return: NotNullIfNotNull("defaultValue")]
#endif
		public static string? Get(string key, string? defaultValue, string? sharedName) =>
			Current.Get<string?>(key, defaultValue, sharedName);

		/// <inheritdoc cref="IPreferences.Get{T}(string, T, string?)"/>
		public static bool Get(string key, bool defaultValue, string? sharedName) =>
			Current.Get<bool>(key, defaultValue, sharedName);

		/// <inheritdoc cref="IPreferences.Get{T}(string, T, string?)"/>
		public static int Get(string key, int defaultValue, string? sharedName) =>
			Current.Get<int>(key, defaultValue, sharedName);

		/// <inheritdoc cref="IPreferences.Get{T}(string, T, string?)"/>
		public static double Get(string key, double defaultValue, string? sharedName) =>
			Current.Get<double>(key, defaultValue, sharedName);

		/// <inheritdoc cref="IPreferences.Get{T}(string, T, string?)"/>
		public static float Get(string key, float defaultValue, string? sharedName) =>
			Current.Get<float>(key, defaultValue, sharedName);

		/// <inheritdoc cref="IPreferences.Get{T}(string, T, string?)"/>
		public static long Get(string key, long defaultValue, string? sharedName) =>
			Current.Get<long>(key, defaultValue, sharedName);

		/// <inheritdoc cref="IPreferences.Set{T}(string, T, string?)"/>
		public static void Set(string key, string? value, string? sharedName) =>
			Current.Set<string?>(key, value, sharedName);

		/// <inheritdoc cref="IPreferences.Set{T}(string, T, string?)"/>
		public static void Set(string key, bool value, string? sharedName) =>
			Current.Set<bool>(key, value, sharedName);

		/// <inheritdoc cref="IPreferences.Set{T}(string, T, string?)"/>
		public static void Set(string key, int value, string? sharedName) =>
			Current.Set<int>(key, value, sharedName);

		/// <inheritdoc cref="IPreferences.Set{T}(string, T, string?)"/>
		public static void Set(string key, double value, string? sharedName) =>
			Current.Set<double>(key, value, sharedName);

		/// <inheritdoc cref="IPreferences.Set{T}(string, T, string?)"/>
		public static void Set(string key, float value, string? sharedName) =>
			Current.Set<float>(key, value, sharedName);

		/// <inheritdoc cref="IPreferences.Set{T}(string, T, string?)"/>
		public static void Set(string key, long value, string? sharedName) =>
			Current.Set<long>(key, value, sharedName);

		// DateTime

		/// <inheritdoc cref="Get(string, string?)"/>
		public static DateTime Get(string key, DateTime defaultValue) =>
			Get(key, defaultValue, null);

		/// <inheritdoc cref="Set(string, string?)"/>
		public static void Set(string key, DateTime value) =>
			Set(key, value, null);

		/// <inheritdoc cref="Get(string, string?)"/>
		public static DateTimeOffset Get(string key, DateTimeOffset defaultValue) =>
			Get(key, defaultValue, null);

		/// <inheritdoc cref="Set(string, string?)"/>
		public static void Set(string key, DateTimeOffset value) =>
			Set(key, value, null);

		/// <inheritdoc cref="IPreferences.Get{T}(string, T, string?)"/>
		public static DateTime Get(string key, DateTime defaultValue, string? sharedName) =>
			Current.Get<DateTime>(key, defaultValue, sharedName);

		/// <inheritdoc cref="IPreferences.Set{T}(string, T, string?)"/>
		public static void Set(string key, DateTime value, string? sharedName) =>
			Current.Set<DateTime>(key, value, sharedName);

		/// <inheritdoc cref="IPreferences.Get{T}(string, T, string?)"/>
		public static DateTimeOffset Get(string key, DateTimeOffset defaultValue, string? sharedName) =>
			Current.Get<DateTimeOffset>(key, defaultValue, sharedName);

		/// <inheritdoc cref="IPreferences.Set{T}(string, T, string?)"/>
		public static void Set(string key, DateTimeOffset value, string? sharedName) =>
			Current.Set<DateTimeOffset>(key, value, sharedName);

		static IPreferences Current => Storage.Preferences.Default;

		internal static string GetPrivatePreferencesSharedName(string feature) =>
			$"{ApplicationModel.AppInfo.Current.PackageName}.microsoft.maui.essentials.{feature}";

		static IPreferences? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IPreferences Default =>
			defaultImplementation ??= new PreferencesImplementation();

		internal static void SetDefault(IPreferences? implementation) =>
			defaultImplementation = implementation;

		internal static Type[] SupportedTypes = new Type[]
		{
			typeof(string),
			typeof(int),
			typeof(bool),
			typeof(long),
			typeof(double),
			typeof(float),
			typeof(DateTime),
			typeof(DateTimeOffset)
		};

		internal static void CheckIsSupportedType<T>()
		{
			var type = typeof(T);
			if (!SupportedTypes.Contains(type))
			{
				throw new NotSupportedException($"Preferences using '{type}' type is not supported");
			}
		}
	}
}
