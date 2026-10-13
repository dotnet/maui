using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Maui.HybridApp;

/// <summary>Explicit, typed command and event-topic registrations for one native host.</summary>
public sealed class CommandRegistry
{
	readonly Dictionary<string, Func<JsonElement, Operation, Task<JsonElement>>> _commands = new(StringComparer.Ordinal);
	readonly HashSet<string> _disabled = new(StringComparer.Ordinal);
	readonly HashSet<string> _topics = new(StringComparer.Ordinal);

	/// <summary>Gets the registered command names, including explicitly disabled commands.</summary>
	public string[] Names => _commands.Keys.Order(StringComparer.Ordinal).ToArray();

	/// <summary>Allows an event topic. Duplicate registrations are rejected.</summary>
	/// <param name="name">The topic name chosen by its provider.</param>
	public void RegisterTopic(string name)
	{
		ValidateName(name);
		if (!_topics.Add(name))
			throw new InvalidOperationException($"Duplicate topic registration: {name}");
	}

	/// <summary>Checks whether the host explicitly registered a topic.</summary>
	/// <param name="name">The topic name.</param>
	/// <returns>Whether this topic is available.</returns>
	public bool HasTopic(string name) => _topics.Contains(name);

	/// <summary>Registers a command with provider-owned, strict source-generated JSON metadata.</summary>
	/// <typeparam name="TArgs">The provider's input contract.</typeparam>
	/// <typeparam name="TResult">The provider's output contract.</typeparam>
	/// <param name="name">A unique command name, at most 64 ASCII letters, digits, dots, underscores or hyphens.</param>
	/// <param name="input">Source-generated input metadata with strict constructor, member and nullability validation.</param>
	/// <param name="output">Source-generated output metadata.</param>
	/// <param name="handler">The typed handler. Honor cancellation before and after native side effects.</param>
	/// <param name="enabled">Whether host policy enables this command; this is not an OS permission.</param>
	public void Register<TArgs, TResult>(string name, JsonTypeInfo<TArgs> input, JsonTypeInfo<TResult> output,
		Func<TArgs, Operation, Task<TResult>> handler, bool enabled = true)
	{
		ValidateName(name);
		ArgumentNullException.ThrowIfNull(handler);
		ValidateMetadata(input);
		ValidateMetadata(output);
		if (!_commands.TryAdd(name, async (json, operation) =>
		{
			TArgs args;
			try
			{
				args = json.Deserialize(input) ?? throw new JsonException("Arguments must not be null.");
			}
			catch (JsonException)
			{
				throw new CommandException("invalid_arguments", "Arguments do not match the declared command contract.");
			}
			operation.Cancellation.ThrowIfCancellationRequested();
			var result = await handler(args, operation).ConfigureAwait(false);
			operation.Cancellation.ThrowIfCancellationRequested();
			return JsonSerializer.SerializeToElement(result, output);
		}))
			throw new InvalidOperationException($"Duplicate command registration: {name}");
		if (!enabled)
			_disabled.Add(name);
	}

	/// <summary>Dispatches a registered command, rejecting unknown or disabled commands.</summary>
	/// <param name="name">The registered name.</param>
	/// <param name="args">JSON arguments validated using the provider's metadata.</param>
	/// <param name="operation">The current session, cancellation and event boundary.</param>
	/// <returns>The result serialized using the provider's metadata.</returns>
	public Task<JsonElement> DispatchAsync(string name, JsonElement args, Operation operation)
	{
		if (!_commands.TryGetValue(name, out var handler))
			throw new CommandException("unknown_command", "This command is not registered.");
		if (_disabled.Contains(name))
			throw new CommandException("disabled_command", "This command is disabled by host policy, not an OS permission.");
		return handler(args, operation);
	}

	static void ValidateName(string name)
	{
		ArgumentException.ThrowIfNullOrEmpty(name);
		if (name.Length > 64 || name.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '.' and not '_' and not '-'))
			throw new ArgumentException("Names must be 1–64 ASCII letters, digits, dots, underscores or hyphens.", nameof(name));
	}

	static void ValidateMetadata(JsonTypeInfo info)
	{
		ArgumentNullException.ThrowIfNull(info);
		if (info.OriginatingResolver is not JsonSerializerContext ||
			info.Options.UnmappedMemberHandling != JsonUnmappedMemberHandling.Disallow ||
			!info.Options.RespectNullableAnnotations || !info.Options.RespectRequiredConstructorParameters)
			throw new ArgumentException("Use strict source-generated JSON metadata owned by the provider.", nameof(info));
	}
}
