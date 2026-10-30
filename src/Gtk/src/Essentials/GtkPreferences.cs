using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IPreferences"/> for Linux: one JSON file per shared name,
/// <c>$XDG_CONFIG_HOME/&lt;app&gt;/preferences.json</c> (default) or <c>preferences.&lt;sharedName&gt;.json</c>.
/// </summary>
/// <remarks>
/// No GSettings schema is required. The file is read lazily and cached, so edits made by another process while the
/// app runs are not observed. Supported types: <see cref="string"/>, <see cref="bool"/>, <see cref="int"/>,
/// <see cref="long"/>, <see cref="float"/>, <see cref="double"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>.
/// </remarks>
sealed class GtkPreferences : IPreferences
{
	readonly string directory;
	readonly ConcurrentDictionary<string, JsonPreferencesStore> stores = new(StringComparer.Ordinal);

	internal GtkPreferences(string applicationId, XdgPaths? xdg = null)
		: this(Path.Combine((xdg ?? XdgPaths.Default).ConfigHome, AppIdentity.ToDirectoryName(applicationId)))
	{
	}

	/// <summary>Stores the files directly under <paramref name="directory"/>.</summary>
	internal GtkPreferences(string directory) => this.directory = directory;

	public bool ContainsKey(string key, string? sharedName = null) =>
		Store(sharedName).ContainsKey(key);

	public void Remove(string key, string? sharedName = null) =>
		Store(sharedName).Remove(key);

	public void Clear(string? sharedName = null) =>
		Store(sharedName).Clear();

	public void Set<T>(string key, T value, string? sharedName = null)
	{
		var store = Store(sharedName);

		if (value is null)
		{
			store.Remove(key);
			return;
		}

		store.Set(key, Encode(value));
	}

	public T Get<T>(string key, T defaultValue, string? sharedName = null)
	{
		if (!Store(sharedName).TryGet(key, out var stored) || stored is null)
			return defaultValue;

		try
		{
			return Decode<T>(stored, defaultValue);
		}
		catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
		{
			return defaultValue;
		}
	}

	JsonPreferencesStore Store(string? sharedName) =>
		stores.GetOrAdd(sharedName ?? string.Empty, name => new JsonPreferencesStore(Path.Combine(directory, FileNameFor(name))));

	internal static string FileNameFor(string sharedName)
	{
		if (sharedName.Length == 0)
			return "preferences.json";

		var sb = new StringBuilder("preferences.");
		var invalid = Path.GetInvalidFileNameChars();
		foreach (var c in sharedName)
			sb.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);

		return sb.Append(".json").ToString();
	}

	static object Encode<T>(T value) => value switch
	{
		string s => s,
		bool b => b,
		int i => (long)i,
		long l => l,
		float f => float.IsFinite(f) ? (object)(double)f : f.ToString("R", CultureInfo.InvariantCulture),
		double d => double.IsFinite(d) ? (object)d : d.ToString("R", CultureInfo.InvariantCulture),
		DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
		DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
		_ => throw new NotSupportedException($"Preferences do not support values of type {typeof(T)}."),
	};

	static T Decode<T>(object stored, T defaultValue)
	{
		var type = typeof(T);

		if (type == typeof(string))
			return (T)(object)(stored as string ?? Convert.ToString(stored, CultureInfo.InvariantCulture)!);
		if (type == typeof(bool))
			return (T)(object)(stored is bool b ? b : bool.Parse((string)stored));
		if (type == typeof(int))
			return (T)(object)checked((int)ToLong(stored));
		if (type == typeof(long))
			return (T)(object)ToLong(stored);
		if (type == typeof(double))
			return (T)(object)ToDouble(stored);
		if (type == typeof(float))
			return (T)(object)(float)ToDouble(stored);
		if (type == typeof(DateTime))
			return (T)(object)DateTime.Parse((string)stored, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		if (type == typeof(DateTimeOffset))
			return (T)(object)DateTimeOffset.Parse((string)stored, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

		throw new NotSupportedException($"Preferences do not support values of type {typeof(T)}.");
	}

	static long ToLong(object stored) => stored switch
	{
		long l => l,
		double d => checked((long)d),
		string s => long.Parse(s, CultureInfo.InvariantCulture),
		_ => throw new InvalidCastException(),
	};

	static double ToDouble(object stored) => stored switch
	{
		double d => d,
		long l => l,
		string s => double.Parse(s, CultureInfo.InvariantCulture),
		_ => throw new InvalidCastException(),
	};
}
