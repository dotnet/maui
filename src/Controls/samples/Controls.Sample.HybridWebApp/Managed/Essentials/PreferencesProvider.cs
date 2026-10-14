using Microsoft.Maui.Storage;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;

namespace Maui.HybridApp.Essentials.Preferences;

/// <summary>Native primitive preferences restricted to a required host-chosen prefix.</summary>
public sealed class PreferencesProvider
{
	readonly IPreferences _preferences;
	readonly string _keyPrefix;
	readonly int _maximumTextLength;

	/// <summary>Creates a namespace-restricted provider without exposing the entire preference store.</summary>
	/// <param name="preferences">The host's existing Essentials preference API.</param>
	/// <param name="options">The immutable host-owned key prefix and text budget.</param>
	/// <remarks>Keys are limited to 1–48 ASCII letters, digits, underscores or hyphens. Clear/shared containers stay namespace-isolated.</remarks>
	public PreferencesProvider(IPreferences preferences, HybridAppOptions options)
	{
		ArgumentNullException.ThrowIfNull(preferences);
		ArgumentNullException.ThrowIfNull(options);
		_preferences = preferences;
		_keyPrefix = options.PreferenceNamespace;
		_maximumTextLength = options.MaximumTextLength;
	}

	[HybridAction("preferences.contains", MainThread = true)]
	internal PreferenceAccepted Contains(string key, string? sharedName = null)
	{
		ValidateKey(key);
		return new(_preferences.ContainsKey(_keyPrefix + key, Container(sharedName)));
	}

	[HybridAction("preferences.get", MainThread = true)]
	internal PreferenceScalar Get(string key, PreferenceScalar defaultValue, string? sharedName = null)
	{
		ValidateKey(key);
		return ReadScalar(_keyPrefix + key, defaultValue, Container(sharedName));
	}

	[HybridAction("preferences.set", MainThread = true)]
	internal PreferenceAccepted Set(string key, PreferenceScalar value, string? sharedName = null)
	{
		ValidateKey(key);
		var container = Container(sharedName);
		var write = PrepareScalarWrite(_keyPrefix + key, value, container);
		WriteOwned(key, container, write);
		return new(true);
	}

	[HybridAction("preferences.remove", MainThread = true)]
	internal PreferenceAccepted Remove(string key, string? sharedName = null)
	{
		ValidateKey(key);
		RemoveOwned(key, Container(sharedName));
		return new(true);
	}

	[HybridAction("preferences.clear", MainThread = true)]
	internal PreferenceAccepted Clear(string? sharedName = null)
	{
		var container = Container(sharedName);
		foreach (var key in OwnedKeys(container)) _preferences.Remove(_keyPrefix + key, container);
		_preferences.Remove(_keyPrefix + "@owned-keys", container);
		return new(true);
	}

	static void ValidateKey(string key)
	{
		if (key.Length is < 1 or > 48 || key.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '_' and not '-'))
			throw new CommandException("invalid_arguments", "Keys must be 1–48 ASCII letters, digits, underscores or hyphens. Only the host-chosen namespace is accessible.");
	}

	void ValidateText(string text)
	{
		if (text.Length > _maximumTextLength)
			throw new CommandException("invalid_arguments", $"Text is limited to {_maximumTextLength} UTF-16 code units.");
	}
	string? Container(string? sharedName)
	{
		if (sharedName is null)
			return null;
		if (sharedName.Length is < 1 or > 128 || sharedName.Any(char.IsControl))
			throw new CommandException("invalid_arguments", "Use a nonempty shared preference container name without control characters.");
		return _keyPrefix + sharedName;
	}
	string[] OwnedKeys(string? container)
	{
		var json = _preferences.Get(_keyPrefix + "@owned-keys", "[]", container);
		try
		{ return JsonSerializer.Deserialize(json, PreferencesJsonContext.Default.StringArray) ?? []; }
		catch (JsonException) { throw new CommandException("native_error", "The owned preference-key index is invalid."); }
	}
	void WriteOwned(string key, string? container, Action write)
	{
		var keys = OwnedKeys(container).ToHashSet(StringComparer.Ordinal);
		if (!keys.Add(key))
		{
			write();
			return;
		}
		if (keys.Count > 4096)
			throw new CommandException("resource_limit", "The host preference namespace key budget was reached.");
		var previouslyPersisted = _preferences.ContainsKey(_keyPrefix + key, container);
		// Reserve ownership first so a failed/budget-rejected write cannot leave an untracked value.
		SaveOwnedKeys(keys, container);
		try
		{
			write();
		}
		catch
		{
			if (!previouslyPersisted)
			{
				_preferences.Remove(_keyPrefix + key, container);
				keys.Remove(key);
				SaveOwnedKeys(keys, container);
			}
			throw;
		}
	}
	void RemoveOwned(string key, string? container)
	{
		_preferences.Remove(_keyPrefix + key, container);
		var keys = OwnedKeys(container).ToHashSet(StringComparer.Ordinal);
		if (keys.Remove(key))
			SaveOwnedKeys(keys, container);
	}
	void SaveOwnedKeys(HashSet<string> keys, string? container) =>
		_preferences.Set(_keyPrefix + "@owned-keys", JsonSerializer.Serialize(keys.Order(StringComparer.Ordinal).ToArray(),
			PreferencesJsonContext.Default.StringArray), container);
	PreferenceScalar ReadScalar(string key, PreferenceScalar value, string? container) => value.Kind switch
	{
		PreferenceKind.String => new(PreferenceKind.String, CheckedText(_preferences.Get(key, CheckedText(value.Text), container))),
		PreferenceKind.Boolean => new(PreferenceKind.Boolean, Boolean: _preferences.Get(key, value.Boolean ?? throw InvalidScalar(), container)),
		PreferenceKind.Double => new(PreferenceKind.Double, Number: _preferences.Get(key, Finite(value.Number), container)),
		PreferenceKind.Int => new(PreferenceKind.Int, Integer: _preferences.Get(key, checked((int)(value.Integer ?? throw InvalidScalar())), container)),
		PreferenceKind.Long => new(PreferenceKind.Long, Integer: _preferences.Get(key, value.Integer ?? throw InvalidScalar(), container)),
		PreferenceKind.Float => new(PreferenceKind.Float, Number: _preferences.Get(key, (float)Finite(value.Number), container)),
		PreferenceKind.DateTime => new(PreferenceKind.DateTime, _preferences.Get(key, DateTime.Parse(value.Text ?? throw InvalidScalar(), CultureInfo.InvariantCulture,
			DateTimeStyles.RoundtripKind), container).ToString("O", CultureInfo.InvariantCulture)),
		_ => throw InvalidScalar(),
	};
	Action PrepareScalarWrite(string key, PreferenceScalar value, string? container) => value.Kind switch
	{
		PreferenceKind.String => Setter(key, CheckedText(value.Text), container),
		PreferenceKind.Boolean => Setter(key, value.Boolean ?? throw InvalidScalar(), container),
		PreferenceKind.Double => Setter(key, Finite(value.Number), container),
		PreferenceKind.Int => Setter(key, CheckedInt(value.Integer), container),
		PreferenceKind.Long => Setter(key, value.Integer ?? throw InvalidScalar(), container),
		PreferenceKind.Float => Setter(key, CheckedFloat(value.Number), container),
		PreferenceKind.DateTime => Setter(key, CheckedDate(value.Text), container),
		_ => throw InvalidScalar(),
	};
	Action Setter<T>(string key, T value, string? container) => () => _preferences.Set(key, value, container);
	string CheckedText(string? value)
	{
		if (value is null)
			throw InvalidScalar();
		ValidateText(value);
		return value;
	}
	static int CheckedInt(long? value) => value is >= int.MinValue and <= int.MaxValue ? (int)value.Value : throw InvalidScalar();
	static float CheckedFloat(double? value)
	{
		var number = (float)Finite(value);
		return float.IsFinite(number) ? number : throw InvalidScalar();
	}
	static DateTime CheckedDate(string? value) =>
		value is not null && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date)
			? date : throw InvalidScalar();
	static double Finite(double? value) => value is double number && double.IsFinite(number) ? number : throw InvalidScalar();
	static CommandException InvalidScalar() => new("invalid_arguments", "Use a native supported preference primitive and matching value.");
}

enum PreferenceKind { String, Boolean, Double, Int, Long, Float, DateTime }
sealed record PreferenceScalar(PreferenceKind Kind, string? Text = null, bool? Boolean = null, double? Number = null, long? Integer = null);
sealed record PreferenceAccepted(bool Accepted);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(string[]))]
partial class PreferencesJsonContext : JsonSerializerContext;
