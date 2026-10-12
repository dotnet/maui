using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class CommandRegistry
{
	readonly Dictionary<string, Func<JsonElement, Operation, Task<JsonElement>>> _commands = new(StringComparer.Ordinal);
	readonly HashSet<string> _disabled = new(StringComparer.Ordinal);
	readonly HashSet<string> _topics = new(StringComparer.Ordinal);

	public string[] Names => _commands.Keys.Order(StringComparer.Ordinal).ToArray();

	public void RegisterTopic(string name)
	{
		if (!_topics.Add(name))
			throw new InvalidOperationException($"Duplicate topic registration: {name}");
	}

	public bool HasTopic(string name) => _topics.Contains(name);

	public void Register<TArgs, TResult>(string name, JsonTypeInfo<TArgs> input, JsonTypeInfo<TResult> output,
		Func<TArgs, Operation, Task<TResult>> handler, bool enabled = true)
	{
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

	public Task<JsonElement> DispatchAsync(string name, JsonElement args, Operation operation)
	{
		if (!_commands.TryGetValue(name, out var handler))
			throw new CommandException("unknown_command", "This command is not registered.");
		if (_disabled.Contains(name))
			throw new CommandException("disabled_command", "This command is disabled by sample policy, not an OS permission.");
		return handler(args, operation);
	}
}

sealed record Operation(string Session, string Id, CancellationToken Cancellation, Action<string, JsonElement> Publish);
