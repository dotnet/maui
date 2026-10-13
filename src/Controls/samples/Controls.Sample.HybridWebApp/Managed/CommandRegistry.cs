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
	readonly Dictionary<string, (Func<Task> Start, Action Stop)> _topicSources = new(StringComparer.Ordinal);
	readonly HashSet<string> _interactive = new(StringComparer.Ordinal);

	/// <summary>Gets commands explicitly permitted to await OS interaction for up to five minutes.</summary>
	public string[] InteractiveCommands => _interactive.Order(StringComparer.Ordinal).ToArray();

	/// <summary>Checks the registered execution policy, not the command's name.</summary>
	/// <param name="name">An explicitly registered command.</param>
	/// <returns>Whether this command may hold the document lease while awaiting native UI.</returns>
	public bool IsInteractive(string name) => _interactive.Contains(name);

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
	/// <summary>Registers an explicit native event source whose first/last document subscription owns OS handlers.</summary>
	/// <param name="name">The registered topic.</param>
	/// <param name="start">Starts the scoped native source; must handle UI-thread dispatch.</param>
	/// <param name="stop">Invalidates queued delivery and detaches this source on unsubscribe/reset/disposal.</param>
	public void RegisterTopic(string name, Func<Task> start, Action stop)
	{
		ArgumentNullException.ThrowIfNull(start); ArgumentNullException.ThrowIfNull(stop);
		RegisterTopic(name);
		_topicSources.Add(name, (start, stop));
	}
	internal Task StartTopicAsync(string name) => _topicSources.TryGetValue(name, out var source) ? source.Start() : Task.CompletedTask;
	internal void StopTopic(string name) { if (_topicSources.TryGetValue(name, out var source)) source.Stop(); }

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
	/// <param name="interactive">Whether this command may await OS UI for a bounded five-minute budget instead of twenty seconds.</param>
	public void Register<TArgs, TResult>(string name, JsonTypeInfo<TArgs> input, JsonTypeInfo<TResult> output,
		Func<TArgs, Operation, Task<TResult>> handler, bool enabled = true, bool interactive = false)
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
		else if (interactive)
			_interactive.Add(name);
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

	internal static void ValidateMetadata(JsonTypeInfo info)
	{
		ArgumentNullException.ThrowIfNull(info);
		if (info.OriginatingResolver is not JsonSerializerContext ||
			info.Options.UnmappedMemberHandling != JsonUnmappedMemberHandling.Disallow ||
			!info.Options.RespectNullableAnnotations || !info.Options.RespectRequiredConstructorParameters)
			throw new ArgumentException("Use strict source-generated JSON metadata owned by the provider.", nameof(info));
	}
}
