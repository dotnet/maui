using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maui.HybridApp;

/// <summary>Explicit compile-time assembly composition used by generated extensions.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HybridActionAssemblies
{
	/// <summary>Adds a generated assembly's explicit registration once, without reflection or global service caches.</summary>
	/// <param name="services">The owning host's registrations.</param>
	/// <param name="identity">The generated assembly registration type.</param>
	/// <param name="register">Compiler-generated direct action and event bindings.</param>
	/// <returns>The same collection.</returns>
	public static IServiceCollection Add(IServiceCollection services, Type identity, Action<CommandRegistry, IServiceProvider> register)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(identity);
		ArgumentNullException.ThrowIfNull(register);
		var configuration = Configuration(services);
		configuration.Assemblies.TryAdd(identity, register);
		Target(services, _ => new NativeDispatcher());
		Target(services, provider =>
		{
			var registry = new CommandRegistry();
			foreach (var assembly in configuration.Assemblies.Values) assembly(registry, provider);
			return registry;
		}, perViewOnly: true);
		return services;
	}
	internal static HybridLocalConfiguration Configuration(IServiceCollection services)
	{
		var configuration = services.Select(d => d.ImplementationInstance).OfType<HybridLocalConfiguration>().SingleOrDefault();
		if (configuration is not null) return configuration;
		configuration = new(services);
		services.AddSingleton(configuration);
		return configuration;
	}

	/// <summary>Records a generated typed constructor without adding its target to the global host collection.</summary>
	/// <typeparam name="T">The local target.</typeparam>
	/// <param name="services">The composing host collection.</param><param name="factory">A typed proxy-aware constructor.</param>
	/// <param name="perViewOnly">Rejects global singleton overrides for targets that capture view/session resources.</param>
	public static void Target<T>(IServiceCollection services, Func<IServiceProvider, T> factory, bool perViewOnly = false) where T : class
	{
		var configuration = Configuration(services);
		configuration.Factories.TryAdd(typeof(T), provider => factory(provider));
		if (perViewOnly) configuration.PerViewOnly.Add(typeof(T));
	}
}

/// <summary>Strict named-object or positional-array binding used only by compiler-generated action handlers.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class HybridActionArguments
{
	readonly JsonElement _json;
	readonly string[] _names;

	/// <summary>Validates the whole argument envelope before deserializing individual typed values.</summary>
	/// <param name="json">A named object or positional array, never a bare scalar.</param>
	/// <param name="names">Only ordinary JSON parameters, excluding service, context and cancellation parameters.</param>
	public HybridActionArguments(JsonElement json, string[] names)
	{
		_json = json; _names = names;
		if (json.ValueKind == JsonValueKind.Array)
		{
			if (json.GetArrayLength() > names.Length) throw Invalid();
		}
		else if (json.ValueKind == JsonValueKind.Object)
		{
			var seen = new HashSet<string>(StringComparer.Ordinal);
			foreach (var property in json.EnumerateObject())
				if (!names.Contains(property.Name, StringComparer.Ordinal) || !seen.Add(property.Name)) throw Invalid();
		}
		else throw Invalid();
	}

	/// <summary>Gets whether an optional argument was supplied.</summary>
	/// <param name="index">The ordinary parameter's position.</param>
	/// <returns>Whether that argument exists.</returns>
	public bool Has(int index) => _json.ValueKind == JsonValueKind.Array ? index < _json.GetArrayLength() : _json.TryGetProperty(_names[index], out _);

	/// <summary>Reads a required value with generated metadata and explicit parameter nullability.</summary>
	/// <typeparam name="T">The ordinary parameter type.</typeparam>
	/// <param name="index">The ordinary parameter's position.</param>
	/// <param name="metadata">Strict compiler-generated type metadata.</param>
	/// <param name="nullable">Whether this parameter permits JSON null.</param>
	/// <returns>The typed value.</returns>
	public T Read<T>(int index, JsonTypeInfo<T> metadata, bool nullable)
	{
		if (!Has(index)) throw Invalid();
		var value = _json.ValueKind == JsonValueKind.Array ? _json[index] : _json.GetProperty(_names[index]);
		if (value.ValueKind == JsonValueKind.Null && !nullable) throw Invalid();
		try { return value.Deserialize(metadata)!; }
		catch (Exception error) when (error is JsonException or FormatException or OverflowException or InvalidOperationException)
		{ throw Invalid(); }
	}
	static CommandException Invalid() => new("invalid_arguments", "Arguments do not match the declared action contract.");
}
