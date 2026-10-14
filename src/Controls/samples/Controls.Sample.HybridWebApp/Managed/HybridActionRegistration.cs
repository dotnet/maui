using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maui.HybridApp;

/// <summary>Generated registration infrastructure. Hosts opt in with <c>AddHybridActions&lt;T&gt;()</c>.</summary>
/// <typeparam name="T">The attributed provider type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HybridActionRegistration<T> where T : class
{
	sealed record Definition(Action<CommandRegistry, IServiceProvider?, T?, NativeDispatcher?> Register, bool RequiresInstance);
	static Definition? _definition;

	/// <summary>Installs the compiler-generated, direct-call registration for one provider assembly.</summary>
	/// <param name="register">The generated registration; no assembly or method discovery is performed.</param>
	/// <param name="requiresInstance">Whether instance actions or a manually implemented source require scoped provider registration.</param>
	public static void Initialize(Action<CommandRegistry, IServiceProvider?, T?, NativeDispatcher?> register, bool requiresInstance = true)
	{
		ArgumentNullException.ThrowIfNull(register);
		if (Interlocked.CompareExchange(ref _definition, new Definition(register, requiresInstance), null) is not null)
			throw new InvalidOperationException("Generated hybrid actions were initialized twice.");
	}

	/// <summary>Registers actions for explicit direct composition, preserving provider constructors and manual topic ownership.</summary>
	/// <param name="registry">The owning command registry.</param>
	/// <param name="instance">The explicitly supplied provider.</param>
	/// <param name="dispatcher">Its host-thread boundary, if any actions require dispatch.</param>
	public static void Register(CommandRegistry registry, T instance, NativeDispatcher? dispatcher = null) =>
		GetRegistration()(registry, null, instance, dispatcher);

	internal static Action<CommandRegistry, IServiceProvider?, T?, NativeDispatcher?> GetRegistration() =>
		GetDefinition().Register;
	internal static bool RequiresInstance => GetDefinition().RequiresInstance;
	static Definition GetDefinition() =>
		_definition ?? throw new InvalidOperationException($"No generated actions for {typeof(T)}. Reference the Maui.HybridApp analyzer when compiling the provider assembly.");
}

/// <summary>Optional manually implemented source lifecycle for an attributed action provider.</summary>
/// <remarks>Action generation does not invent event topics, native event handlers or resource ownership.</remarks>
public interface IHybridActionSource
{
	/// <summary>Registers the provider's explicit event topics and first/last subscription callbacks.</summary>
	/// <param name="registry">The owning document command registry.</param>
	void RegisterTopics(CommandRegistry registry);
}

/// <summary>Explicit host opt-in to the actions generated in a provider's own assembly.</summary>
public static class HybridActionServiceCollectionExtensions
{
	/// <summary>Adds one provider's generated actions, resolving each instance from the current owning scope.</summary>
	/// <typeparam name="T">An accessible provider with generated actions and host-registered constructor dependencies.</typeparam>
	/// <param name="services">The host's registrations. Existing explicit provider factories are preserved.</param>
	/// <returns>The same service collection.</returns>
	/// <remarks>Static-only providers require no instance registration. Repeated opt-in is idempotent.</remarks>
	public static IServiceCollection AddHybridActions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
		this IServiceCollection services) where T : class
	{
		ArgumentNullException.ThrowIfNull(services);
		_ = HybridActionRegistration<T>.GetRegistration();
		if (HybridActionRegistration<T>.RequiresInstance)
			services.TryAddScoped<T>();
		services.TryAddScoped<NativeDispatcher>();
		services.TryAddEnumerable(ServiceDescriptor.Scoped<IHybridCommandProvider, ActionProvider<T>>());
		return services;
	}

	sealed class ActionProvider<T>(IServiceProvider services) : IHybridCommandProvider where T : class
	{
		public void Register(CommandRegistry registry)
		{
			// Only manually declared sources are materialized during registration. Actions resolve their owning scope at invocation.
			if (services.GetService<T>() is IHybridActionSource source)
				source.RegisterTopics(registry);
			HybridActionRegistration<T>.GetRegistration()(registry, services, null, null);
		}
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
