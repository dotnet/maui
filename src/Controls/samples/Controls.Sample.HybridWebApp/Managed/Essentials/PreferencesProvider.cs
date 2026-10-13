using Microsoft.Maui.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json.Serialization;

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
	{
		var json = PreferencesJsonContext.Default;
		registry.Register("preferences.read", json.PreferenceArgs, json.PreferenceResult, (args, op) =>
		{
			ValidateKey(args.Key);
			return _dispatcher.RunAsync(() =>
			{
				var name = _keyPrefix + args.Key;
				var value = _preferences.ContainsKey(name) ? _preferences.Get(name, "") : null;
				if (value is not null)
					ValidateText(value);
				return Task.FromResult(new PreferenceResult(args.Key, value));
			}, op.Cancellation);
		});
		registry.Register("preferences.write", json.SetPreferenceArgs, json.PreferenceResult, (args, op) =>
		{
			ValidateKey(args.Key);
			ValidateText(args.Value);
			return SetAsync(args.Key, args.Value, op.Cancellation);
		});
		registry.Register("preferences.remove", json.PreferenceArgs, json.PreferenceResult, (args, op) =>
		{
			ValidateKey(args.Key);
			return SetAsync(args.Key, null, op.Cancellation);
		});
	}

	Task<PreferenceResult> SetAsync(string key, string? value, CancellationToken cancellation) =>
		_dispatcher.RunAsync(() =>
		{
			if (value is null)
				_preferences.Remove(_keyPrefix + key);
			else
				_preferences.Set(_keyPrefix + key, value);
			return Task.FromResult(new PreferenceResult(key, value));
		}, cancellation);

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

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(PreferenceArgs))]
[JsonSerializable(typeof(SetPreferenceArgs))]
[JsonSerializable(typeof(PreferenceResult))]
partial class PreferencesJsonContext : JsonSerializerContext;
