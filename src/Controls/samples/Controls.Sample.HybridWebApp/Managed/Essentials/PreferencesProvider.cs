using Microsoft.Maui.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;

namespace Maui.HybridApp.Essentials.Preferences;

/// <summary>Explicitly registers string preference commands restricted to a required host-chosen prefix.</summary>
public sealed class PreferencesProvider : IHybridCommandProvider
{
	readonly IPreferences _preferences;
	readonly NativeDispatcher _dispatcher;
	readonly string _keyPrefix;
	readonly int _maximumTextLength;

	/// <summary>Creates a namespace-restricted provider without exposing the entire preference store.</summary>
	/// <param name="preferences">The host's existing Essentials preference API.</param>
	/// <param name="dispatcher">The host's main-thread boundary.</param>
	/// <param name="keyPrefix">A nonempty host-owned prefix, applied to every key. It is not a secret.</param>
	/// <param name="maximumTextLength">Maximum UTF-16 code units on reads and writes; defaults to 4096.</param>
	/// <remarks>Keys remain limited to 1–48 ASCII letters, digits, underscores or hyphens. Clear/shared containers are not exposed.</remarks>
	public PreferencesProvider(IPreferences preferences, NativeDispatcher dispatcher, string keyPrefix, int maximumTextLength = 4096)
	{
		ArgumentNullException.ThrowIfNull(preferences);
		ArgumentNullException.ThrowIfNull(dispatcher);
		ArgumentException.ThrowIfNullOrWhiteSpace(keyPrefix);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTextLength);
		_preferences = preferences;
		_dispatcher = dispatcher;
		_keyPrefix = keyPrefix;
		_maximumTextLength = maximumTextLength;
	}

	/// <inheritdoc />
	public void Register(CommandRegistry registry)
		=> HybridActionRegistration<PreferencesProvider>.Register(registry, this, _dispatcher);

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

	[HybridAction("preferences.delete", MainThread = true)]
	internal PreferenceAccepted Delete(string key, string? sharedName = null)
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

	[HybridAction("preferences.read", MainThread = true)]
	internal PreferenceResult Read(string key)
	{
		ValidateKey(key);
		var name = _keyPrefix + key;
		var value = _preferences.ContainsKey(name) ? _preferences.Get(name, "") : null;
		if (value is not null) ValidateText(value);
		return new(key, value);
	}

	[HybridAction("preferences.write", MainThread = true)]
	internal PreferenceResult Write(string key, string value)
	{
		ValidateKey(key);
		ValidateText(value);
		WriteOwned(key, null, () => _preferences.Set(_keyPrefix + key, value));
		return new(key, value);
	}

	[HybridAction("preferences.remove", MainThread = true)]
	internal PreferenceResult Remove(string key)
	{
		ValidateKey(key);
		RemoveOwned(key, null);
		return new(key, null);
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
		"string" => new("string", _preferences.Get(key, value.Text ?? throw InvalidScalar(), container)),
		"boolean" => new("boolean", Boolean: _preferences.Get(key, value.Boolean ?? throw InvalidScalar(), container)),
		"double" => new("double", Number: _preferences.Get(key, Finite(value.Number), container)),
		"int" => new("int", Integer: _preferences.Get(key, checked((int)(value.Integer ?? throw InvalidScalar())), container)),
		"long" => new("long", Integer: _preferences.Get(key, value.Integer ?? throw InvalidScalar(), container)),
		"float" => new("float", Number: _preferences.Get(key, (float)Finite(value.Number), container)),
		"dateTime" => new("dateTime", _preferences.Get(key, DateTime.Parse(value.Text ?? throw InvalidScalar(), CultureInfo.InvariantCulture,
			DateTimeStyles.RoundtripKind), container).ToString("O", CultureInfo.InvariantCulture)),
		_ => throw InvalidScalar(),
	};
	Action PrepareScalarWrite(string key, PreferenceScalar value, string? container) => value.Kind switch
	{
		"string" => Setter(key, CheckedText(value.Text), container),
		"boolean" => Setter(key, value.Boolean ?? throw InvalidScalar(), container),
		"double" => Setter(key, Finite(value.Number), container),
		"int" => Setter(key, CheckedInt(value.Integer), container),
		"long" => Setter(key, value.Integer ?? throw InvalidScalar(), container),
		"float" => Setter(key, CheckedFloat(value.Number), container),
		"dateTime" => Setter(key, CheckedDate(value.Text), container),
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

/// <summary>Explicit namespace-restricted preferences provider composition.</summary>
public static class PreferencesServiceCollectionExtensions
{
	/// <summary>Registers a scoped provider using the host's nonempty namespace and text policy.</summary>
	/// <param name="services">The host's service registrations, including <see cref="HybridAppOptions"/>.</param>
	/// <returns>The same collection for further explicit registrations.</returns>
	/// <remarks>The default interface uses real OS preferences; registering it does not grant permission or expose clear-all.</remarks>
	public static IServiceCollection AddHybridPreferences(this IServiceCollection services)
	{
		services.TryAddSingleton<IPreferences>(_ => Microsoft.Maui.Storage.Preferences.Default);
		services.TryAddScoped<NativeDispatcher>();
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, PreferencesProvider>(provider =>
		{
			var options = provider.GetRequiredService<HybridAppOptions>();
			return new PreferencesProvider(provider.GetRequiredService<IPreferences>(),
				provider.GetRequiredService<NativeDispatcher>(), options.PreferenceNamespace, options.MaximumTextLength);
		}));
		return services;
	}
}

sealed record PreferenceArgs(string Key);
sealed record SetPreferenceArgs(string Key, string Value);
sealed record PreferenceResult(string Key, string? Value);
sealed record PreferenceContainerArgs(string Key, string? SharedName = null);
sealed record PreferenceClearArgs(string? SharedName = null);
sealed record PreferenceScalar(string Kind, string? Text = null, bool? Boolean = null, double? Number = null, long? Integer = null);
sealed record TypedPreferenceArgs(string Key, PreferenceScalar DefaultValue, string? SharedName = null);
sealed record SetTypedPreferenceArgs(string Key, PreferenceScalar Value, string? SharedName = null);
sealed record PreferenceAccepted(bool Accepted);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(PreferenceArgs))]
[JsonSerializable(typeof(SetPreferenceArgs))]
[JsonSerializable(typeof(PreferenceResult))]
[JsonSerializable(typeof(PreferenceContainerArgs))]
[JsonSerializable(typeof(PreferenceClearArgs))]
[JsonSerializable(typeof(PreferenceScalar))]
[JsonSerializable(typeof(TypedPreferenceArgs))]
[JsonSerializable(typeof(SetTypedPreferenceArgs))]
[JsonSerializable(typeof(PreferenceAccepted))]
[JsonSerializable(typeof(string[]))]
partial class PreferencesJsonContext : JsonSerializerContext;
