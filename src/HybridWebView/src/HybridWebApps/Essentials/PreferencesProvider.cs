using System.Globalization;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Controls.HybridWebApps.Essentials.Preferences;

/// <summary>Maps closed JSON scalars to the generic native preference API without changing keys or containers.</summary>
public sealed class PreferencesProvider
{
	readonly IPreferences _preferences;

	/// <summary>Creates an adapter over the host's actual preference store.</summary>
	/// <param name="preferences">The host's existing Essentials preference API.</param>
	public PreferencesProvider(IPreferences preferences)
	{
		ArgumentNullException.ThrowIfNull(preferences);
		_preferences = preferences;
	}

	[HybridAction("preferences.contains", MainThread = true)]
	internal PreferenceAccepted Contains(string key, string? sharedName = null) =>
		new(_preferences.ContainsKey(key, sharedName));

	[HybridAction("preferences.get", MainThread = true)]
	internal PreferenceScalar Get(string key, PreferenceScalar defaultValue, string? sharedName = null) => defaultValue.Kind switch
	{
		PreferenceKind.String => new(PreferenceKind.String, _preferences.Get(key, Text(defaultValue.Text), sharedName)),
		PreferenceKind.Boolean => new(PreferenceKind.Boolean, Boolean: _preferences.Get(key, defaultValue.Boolean ?? throw InvalidScalar(), sharedName)),
		PreferenceKind.Double => new(PreferenceKind.Double, Number: _preferences.Get(key, Finite(defaultValue.Number), sharedName)),
		PreferenceKind.Int => new(PreferenceKind.Int, Integer: _preferences.Get(key, CheckedInt(defaultValue.Integer), sharedName)),
		PreferenceKind.Long => new(PreferenceKind.Long, Integer: _preferences.Get(key, defaultValue.Integer ?? throw InvalidScalar(), sharedName)),
		PreferenceKind.Float => new(PreferenceKind.Float, Number: _preferences.Get(key, CheckedFloat(defaultValue.Number), sharedName)),
		PreferenceKind.DateTime => new(PreferenceKind.DateTime, _preferences.Get(key, CheckedDate(defaultValue.Text), sharedName).ToString("O", CultureInfo.InvariantCulture)),
		_ => throw InvalidScalar(),
	};

	[HybridAction("preferences.set", MainThread = true)]
	internal PreferenceAccepted Set(string key, PreferenceScalar value, string? sharedName = null)
	{
		switch (value.Kind)
		{
			case PreferenceKind.String:
				_preferences.Set(key, Text(value.Text), sharedName);
				break;
			case PreferenceKind.Boolean:
				_preferences.Set(key, value.Boolean ?? throw InvalidScalar(), sharedName);
				break;
			case PreferenceKind.Double:
				_preferences.Set(key, Finite(value.Number), sharedName);
				break;
			case PreferenceKind.Int:
				_preferences.Set(key, CheckedInt(value.Integer), sharedName);
				break;
			case PreferenceKind.Long:
				_preferences.Set(key, value.Integer ?? throw InvalidScalar(), sharedName);
				break;
			case PreferenceKind.Float:
				_preferences.Set(key, CheckedFloat(value.Number), sharedName);
				break;
			case PreferenceKind.DateTime:
				_preferences.Set(key, CheckedDate(value.Text), sharedName);
				break;
			default:
				throw InvalidScalar();
		}
		return new(true);
	}

	[HybridAction("preferences.remove", MainThread = true)]
	internal PreferenceAccepted Remove(string key, string? sharedName = null)
	{
		_preferences.Remove(key, sharedName);
		return new(true);
	}

	[HybridAction("preferences.clear", MainThread = true)]
	internal PreferenceAccepted Clear(string? sharedName = null)
	{
		_preferences.Clear(sharedName);
		return new(true);
	}

	static string Text(string? value) => value ?? throw InvalidScalar();
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
