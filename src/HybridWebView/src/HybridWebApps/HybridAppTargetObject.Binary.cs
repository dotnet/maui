using System.Text.Json;

namespace Microsoft.Maui.Controls.HybridWebApps;

public sealed partial class HybridAppTargetObject
{
	readonly Dictionary<string, HybridChannelState> _channels = new(StringComparer.Ordinal);
	readonly HashSet<string> _controlOperations = new(StringComparer.Ordinal);
	readonly HashSet<string> _binaryOperations = new(StringComparer.Ordinal);

	(CancellationTokenSource Cancellation, CancellationTokenSource Deadline) BeginOperation(Request message, CancellationToken transport, bool binary = false)
	{
		if (message.Version != 1 || !Guid.TryParseExact(message.Session, "D", out _) ||
			!Guid.TryParseExact(message.Id, "D", out _) || message.Command.Length is < 1 or > 64)
			throw new CommandException("invalid_request", "Expected protocol version 1 and UUID identities.");
		lock (_gate)
		{
			if (_disposed)
				throw new CommandException("disconnected", "The native WebView has been destroyed.");
			if (message.Command == "core.connect" && _session != message.Session)
			{
				if (_retiredSessions.Contains(message.Session))
					throw new CommandException("stale_session", "This document has been retired.");
				if (_retiredSessions.Count >= MaximumRetiredDocuments)
					throw new CommandException("document_limit", "Restart the native app.");
				ResetDocument();
				_session = message.Session;
			}
			if (_session != message.Session)
				throw new CommandException("stale_session", "This document is disconnected.");
			_lastSeen = _time.GetUtcNow();
			var control = message.Command is "core.cancel" or "core.unsubscribe" or "core.channel.fetch" or "core.channel.ack" or "core.channel.close";
			if (control ? _controlOperations.Count >= 4 : _operations.Count - _controlOperations.Count >= 16)
				throw new CommandException("busy", control ? "At most four native control calls may be active." : "At most 16 native calls may be active.");
			if (binary && !control && _binaryOperations.Count >= 3)
				throw new CommandException("busy", "At most three binary actions may be active; one Core transfer slot is reserved for channel data.");
			if (_operations.ContainsKey(message.Id) || !_seen.Add(message.Id))
				throw new CommandException("duplicate_request", "Request identity already used.");
			_seenOrder.Enqueue(message.Id);
			if (_seenOrder.Count > 256)
				_seen.Remove(_seenOrder.Dequeue());
			var interactive = _registry.IsInteractive(message.Command);
			var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(interactive ? 300 : 20), _time);
			var cancellation = CancellationTokenSource.CreateLinkedTokenSource(_document.Token, deadline.Token, transport);
			_operations.Add(message.Id, cancellation);
			if (control)
				_controlOperations.Add(message.Id);
			if (binary && !control)
				_binaryOperations.Add(message.Id);
			if (interactive)
				_interactiveOperations.Add(message.Id);
			return (cancellation, deadline);
		}
	}

	Operation CreateOperation(Request message, CancellationToken cancellation, byte[]? payload = null) =>
		new(message.Session, message.Id, cancellation, (topic, data) => Publish(message.Session, message.Id, topic, data))
		{
			Payload = payload,
			ChannelFactory = id => BindChannel(message.Session, message.Id, id, cancellation)
		};

	/// <summary>Dispatches separate bytes under the same document authorization, replay and operation policies as JSON.</summary>
	/// <param name="metadata">ASCII protocol request, at most 2400 characters.</param>
	/// <param name="payload">Separate raw bytes, at most 4 MiB.</param>
	/// <param name="transport">Core transfer cancellation.</param>
	/// <returns>Raw action result bytes. All failures throw; there is no success-shaped error payload.</returns>
	public async Task<byte[]> InvokeBinary(string metadata, byte[] payload, CancellationToken transport = default)
	{
		if (metadata.Length > HybridAppBinaryInvoker.MaximumMetadataCharacters || metadata.Any(c => c > 127))
			throw new CommandException("invalid_request", "Binary action metadata is limited to 2400 ASCII characters.");
		if (payload.Length > HybridAppBinaryInvoker.MaximumBytes)
			throw new CommandException("binary_limit", "Input exceeds 4 MiB.");
		var message = JsonSerializer.Deserialize(metadata, ProtocolJsonContext.Default.Request) ?? throw new JsonException();
		if (message.Command == "core.connect")
			throw new CommandException("invalid_transport", "Connect using JSON before binary calls.");
		var (cancellation, deadline) = BeginOperation(message, transport, binary: true);
		var status = "error";
		using var registration = cancellation.Token.Register(() => FinishChannels(message.Id, "cancelled"));
		try
		{
			// Keep the operation slot until the real handler returns, even if it ignores cancellation.
			var result = await _registry.DispatchBinaryAsync(message.Command, message.Args,
				CreateOperation(message, cancellation.Token, payload)).ConfigureAwait(false);
			cancellation.Token.ThrowIfCancellationRequested();
			status = "complete";
			return result;
		}
		finally
		{
			lock (_gate)
			{
				FinishChannels(message.Id, cancellation.IsCancellationRequested ? "cancelled" : status);
				if (_operations.TryGetValue(message.Id, out var current) && ReferenceEquals(current, cancellation))
				{
					_operations.Remove(message.Id);
					_controlOperations.Remove(message.Id);
					_binaryOperations.Remove(message.Id);
					_interactiveOperations.Remove(message.Id);
					if (_registry.IsInteractive(message.Command))
						_lastSeen = _time.GetUtcNow();
				}
			}
			cancellation.Dispose();
			deadline.Dispose();
		}
	}

	HybridChannelState BindChannel(string session, string owner, string id, CancellationToken cancellation)
	{
		lock (_gate)
		{
			cancellation.ThrowIfCancellationRequested();
			if (!Guid.TryParseExact(id, "D", out _) || _channels.ContainsKey(id))
				throw new CommandException("invalid_channel", "Expected a unique scoped channel UUID.");
			if (_channels.Values.Count(c => c.Owner == owner) >= 4)
				throw new CommandException("channel_limit", "At most four channels may belong to an action.");
			var channel = new HybridChannelState(session, owner, id, cancellation, SendChannel);
			_channels.Add(id, channel);
			return channel;
		}
	}

	async Task SendChannel(HybridChannelState channel, JsonElement? json, byte[]? bytes)
	{
		TaskCompletionSource<bool> pending;
		lock (_gate)
		{
			channel.Cancellation.ThrowIfCancellationRequested();
			if (!_channels.TryGetValue(channel.Id, out var current) || !ReferenceEquals(channel, current))
				throw new CommandException("channel_closed", "The owner action or channel ended.");
			if (channel.Pending is not null)
				throw new CommandException("channel_busy", "Await the previous send before sending again.");
			if (bytes?.Length > 65536)
				throw new CommandException("channel_limit", "A raw channel chunk is limited to 64 KiB.");
			if (channel.Sequence == int.MaxValue - 1)
				throw new CommandException("channel_limit", "Channel sequence exhausted.");
			channel.Sequence++;
			channel.Bytes = bytes?.ToArray();
			channel.DataId = bytes is null ? null : Guid.NewGuid().ToString("D");
			channel.Fetched = false;
			pending = channel.Pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
			SendChannelEvent(channel, new(channel.Id, channel.Sequence, bytes is null ? "json" : "bytes", json, channel.DataId));
		}
		try
		{ await pending.Task.WaitAsync(channel.Cancellation).ConfigureAwait(false); }
		finally
		{
			lock (_gate)
				if (ReferenceEquals(channel.Pending, pending))
				{
					channel.Pending = null;
					channel.Bytes = null;
					channel.DataId = null;
				}
		}
	}

	void SendChannelEvent(HybridChannelState channel, ChannelMessage message)
	{
		if (!_disposed && _session == channel.Session)
			NativeEventReceived?.Invoke(new(1, channel.Session, channel.Owner, "core.channel",
				JsonSerializer.SerializeToElement(message, ProtocolJsonContext.Default.ChannelMessage)));
	}

	HybridChannelState OwnedChannel(string id, string owner, Operation operation)
	{
		if (!_channels.TryGetValue(id, out var channel) || channel.Session != operation.Session || channel.Owner != owner ||
			channel.Cancellation.IsCancellationRequested)
			throw new CommandException("invalid_channel", "Channel is closed or belongs to another operation/document.");
		return channel;
	}

	[HybridAction("core.channel.fetch")]
	internal byte[] FetchChannel(string channelId, string ownerId, string dataId, int sequence, HybridActionContext context)
	{
		lock (_gate)
		{
			var channel = OwnedChannel(channelId, ownerId, new(context.Session, context.Id, context.Cancellation, (_, _) => { }));
			if (channel.Sequence != sequence || channel.DataId != dataId || channel.Bytes is null || channel.Fetched)
				throw new CommandException("invalid_channel", "Unknown, late or replayed channel data.");
			var result = channel.Bytes;
			channel.Bytes = null;
			channel.Fetched = true;
			return result;
		}
	}

	[HybridAction("core.channel.ack")]
	internal Ack AckChannel(string channelId, string ownerId, int sequence, bool accepted, HybridActionContext context)
	{
		lock (_gate)
		{
			var channel = OwnedChannel(channelId, ownerId, new(context.Session, context.Id, context.Cancellation, (_, _) => { }));
			if (channel.Sequence != sequence || channel.Pending is null || channel.Pending.Task.IsCompleted ||
				accepted && channel.DataId is not null && !channel.Fetched)
				throw new CommandException("invalid_channel", "Unknown, premature or replayed consumption acknowledgement.");
			if (accepted)
				channel.Pending.TrySetResult(true);
			else
			{
				channel.Pending.TrySetException(new CommandException("consumer_error", "The JS channel consumer rejected the message."));
				EndChannel(channel, "error");
			}
			return new(true);
		}
	}

	[HybridAction("core.channel.close")]
	internal Ack CloseChannel(string channelId, string ownerId, HybridActionContext context)
	{
		lock (_gate)
		{
			var channel = OwnedChannel(channelId, ownerId, new(context.Session, context.Id, context.Cancellation, (_, _) => { }));
			EndChannel(channel, "cancelled");
			return new(true);
		}
	}

	void FinishChannels(string owner, string status)
	{
		lock (_gate)
			foreach (var channel in _channels.Values.Where(c => c.Owner == owner).ToArray())
				EndChannel(channel, status);
	}

	void EndChannel(HybridChannelState channel, string status)
	{
		_channels.Remove(channel.Id);
		channel.Bytes = null;
		channel.DataId = null;
		channel.Pending?.TrySetException(new CommandException(status == "cancelled" ? "cancelled" : "channel_closed", "The owner action or channel ended."));
		channel.Pending = null;
		SendChannelEvent(channel, new(channel.Id, channel.Sequence + 1, status, Code: status == "error" ? "native_error" : null));
	}
}
