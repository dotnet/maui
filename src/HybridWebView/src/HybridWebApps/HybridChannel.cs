using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Typed, ordered, operation-owned native-to-JS delivery with explicit consumption backpressure.</summary>
/// <typeparam name="T">Compiler-supported JSON contract, or byte[] for raw binary chunks.</typeparam>
/// <remarks>At most one send is in flight per channel. Await SendAsync; concurrent sends reject.
/// JSON messages are limited to 16 KiB ASCII JSON, raw chunks to 64 KiB. Completion is scoped to the owner action.</remarks>
public sealed class HybridChannel<T>
{
	readonly HybridChannelState _state;
	readonly JsonTypeInfo<T>? _metadata;
	internal HybridChannel(HybridChannelState state, JsonTypeInfo<T>? metadata) { _state = state; _metadata = metadata; }

	/// <summary>Sends one message and waits until JS has awaited its consumer and acknowledged it.</summary>
	/// <param name="value">The typed value, never a Stream or native delegate.</param>
	/// <returns>Consumption completion; rejects for consumer failure, cancellation, closed channel or exceeded limits.</returns>
	public Task SendAsync(T value)
	{
		if (value is byte[] bytes && _metadata is null)
			return _state.Send(null, bytes);
		if (_metadata is null)
			throw new CommandException("invalid_arguments", "Channel JSON requires generated metadata.");
		var json = JsonSerializer.SerializeToElement(value, _metadata);
		if (json.GetRawText().Length > 16384)
			throw new CommandException("channel_limit", "A channel JSON message is limited to 16 KiB.");
		return _state.Send(json, null);
	}
}

internal sealed class HybridChannelState(string session, string owner, string id, CancellationToken cancellation,
	Func<HybridChannelState, JsonElement?, byte[]?, Task> send)
{
	internal string Session { get; } = session;
	internal string Owner { get; } = owner;
	internal string Id { get; } = id;
	internal CancellationToken Cancellation { get; } = cancellation;
	internal int Sequence;
	internal string? DataId;
	internal byte[]? Bytes;
	internal TaskCompletionSource<bool>? Pending;
	internal bool Fetched;
	internal Task Send(JsonElement? json, byte[]? bytes) => send(this, json, bytes);
}

/// <summary>Bounded channel control envelope; raw byte data is fetched separately, never embedded.</summary>
/// <param name="ChannelId">Scoped caller channel UUID.</param>
/// <param name="Sequence">Strictly increasing message sequence.</param>
/// <param name="Kind">json, bytes, complete, error or cancelled.</param>
/// <param name="Value">Typed JSON payload only.</param>
/// <param name="DataId">Single-consumption raw byte identifier.</param>
/// <param name="Code">Terminal error code.</param>
public sealed record ChannelMessage(string ChannelId, int Sequence, string Kind, JsonElement? Value = null,
	string? DataId = null, string? Code = null);
