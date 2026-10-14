using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Maui.HybridApp;

/// <summary>Explicit, typed command and event-topic registrations for one native host.</summary>
public sealed class CommandRegistry
{
	readonly Dictionary<string, Func<JsonElement, Operation, Task<JsonElement>>> _commands = new(StringComparer.Ordinal);
	readonly Dictionary<string, Func<JsonElement, Operation, Task<byte[]>>> _binary = new(StringComparer.Ordinal);
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
	public string[] Names => _commands.Keys.Concat(_binary.Keys).Order(StringComparer.Ordinal).ToArray();

	/// <summary>Allows an event topic. Duplicate registrations are rejected.</summary>
	/// <param name="name">The topic name chosen by its provider.</param>
	public void RegisterTopic(string name)
	{
		ValidateName(name);
		if (name == "core.channel") throw new ArgumentException("core.channel is reserved for operation-owned channels.", nameof(name));
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

	/// <summary>Registers a compiler-generated direct-call action with strictly source-originated metadata.</summary>
	/// <param name="name">The unique native wire-command name.</param>
	/// <param name="metadata">Generated metadata for every ordinary argument and result type.</param>
	/// <param name="handler">The generated typed handler and binder, without runtime method discovery.</param>
	/// <param name="interactive">Whether the existing bounded native interaction policy applies.</param>
	/// <param name="enabled">Explicit compiled host availability, independent of OS permission.</param>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void RegisterGenerated(string name, JsonTypeInfo[] metadata, Func<JsonElement, Operation, Task<JsonElement>> handler,
		bool interactive = false, bool enabled = true)
	{
		ValidateName(name);
		if (_binary.ContainsKey(name)) throw new InvalidOperationException($"Duplicate command registration: {name}");
		ArgumentNullException.ThrowIfNull(metadata);
		ArgumentNullException.ThrowIfNull(handler);
		if (metadata.Length == 0)
			throw new ArgumentException("Generated actions must supply source-originated result metadata, including their null result for void actions.", nameof(metadata));
		foreach (var info in metadata) ValidateMetadata(info);
		if (!_commands.TryAdd(name, async (args, operation) =>
		{
			operation.Cancellation.ThrowIfCancellationRequested();
			var result = await handler(args, operation).ConfigureAwait(false);
			operation.Cancellation.ThrowIfCancellationRequested();
			return result;
		}))
			throw new InvalidOperationException($"Duplicate command registration: {name}");
		if (!enabled) _disabled.Add(name);
		else if (interactive) _interactive.Add(name);
	}

	/// <summary>Dispatches a registered command, rejecting unknown or disabled commands.</summary>
	/// <param name="name">The registered name.</param>
	/// <param name="args">JSON arguments validated using the provider's metadata.</param>
	/// <param name="operation">The current session, cancellation and event boundary.</param>
	/// <returns>The result serialized using the provider's metadata.</returns>
	public Task<JsonElement> DispatchAsync(string name, JsonElement args, Operation operation)
	{
		if (_binary.ContainsKey(name))
			throw new CommandException("invalid_transport", "This command requires the binary transport.");
		if (!_commands.TryGetValue(name, out var handler))
			throw new CommandException("unknown_command", "This command is not registered.");
		if (_disabled.Contains(name))
			throw new CommandException("disabled_command", "This command is disabled by host policy, not an OS permission.");
		return handler(args, operation);
	}

	/// <summary>Registers a compiler-generated byte-result action; its payload never enters JSON metadata.</summary>
	/// <param name="name">Explicit action route.</param>
	/// <param name="metadata">Strict generated metadata for structured arguments.</param>
	/// <param name="handler">Generated direct call.</param>
	/// <param name="interactive">Host execution policy.</param>
	/// <param name="enabled">Explicit host availability.</param>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void RegisterGeneratedBinary(string name, JsonTypeInfo[] metadata, Func<JsonElement, Operation, Task<byte[]>> handler,
		bool interactive = false, bool enabled = true)
	{
		ValidateName(name);
		ArgumentNullException.ThrowIfNull(handler);
		ArgumentNullException.ThrowIfNull(metadata);
		foreach (var info in metadata) ValidateMetadata(info);
		if (_commands.ContainsKey(name) || !_binary.TryAdd(name, handler))
			throw new InvalidOperationException($"Duplicate command registration: {name}");
		if (!enabled) _disabled.Add(name);
		else if (interactive) _interactive.Add(name);
	}

	internal async Task<byte[]> DispatchBinaryAsync(string name, JsonElement args, Operation operation)
	{
		if (!_binary.TryGetValue(name, out var handler))
			throw new CommandException(_commands.ContainsKey(name) ? "invalid_transport" : "unknown_command", "No approved binary action exists for this route.");
		if (_disabled.Contains(name)) throw new CommandException("disabled_command", "This command is disabled by host policy.");
		operation.Cancellation.ThrowIfCancellationRequested();
		var result = await handler(args, operation).ConfigureAwait(false);
		operation.Cancellation.ThrowIfCancellationRequested();
		if (result is null || result.Length > HybridAppBinaryInvoker.MaximumBytes)
			throw new CommandException("binary_limit", "A binary result must be non-null and at most 4 MiB.");
		return result;
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
