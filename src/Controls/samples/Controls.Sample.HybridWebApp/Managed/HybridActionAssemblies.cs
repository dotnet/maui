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
		if (!services.Any(descriptor => descriptor.ImplementationInstance is AssemblyActions actions && actions.Identity == identity))
			services.AddSingleton(new AssemblyActions(identity, register));
		services.TryAddScoped<NativeDispatcher>();
		services.TryAddScoped(provider =>
		{
			var registry = new CommandRegistry();
			foreach (var assembly in provider.GetServices<AssemblyActions>())
				assembly.Register(registry, provider);
			return registry;
		});
		return services;
	}
	sealed record AssemblyActions(Type Identity, Action<CommandRegistry, IServiceProvider> Register);
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
