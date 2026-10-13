using System.Text.Json;

namespace Maui.HybridApp;

/// <summary>Owns the bounded document session and command lifetime of one WebView without executing UI operations itself.</summary>
public sealed class HybridAppTargetObject : IHybridAppTarget, IDisposable
{
	const int MaximumRequestLength = 100000;
	const int MaximumRetiredDocuments = 4096;
	readonly object _gate = new();
	readonly CommandRegistry _registry;
	readonly Func<ReadyResult> _ready;
	readonly Dictionary<string, CancellationTokenSource> _operations = new(StringComparer.Ordinal);
	readonly HashSet<string> _interactiveOperations = new(StringComparer.Ordinal);
	readonly HashSet<string> _subscriptions = new(StringComparer.Ordinal);
	readonly HashSet<string> _seen = new(StringComparer.Ordinal);
	readonly HashSet<string> _retiredSessions = new(StringComparer.Ordinal);
	readonly Queue<string> _seenOrder = new();
	readonly ITimer _lease;
	readonly TimeProvider _time;
	CancellationTokenSource _document = new();
	string? _session;
	DateTimeOffset _lastSeen;
	bool _disposed;
	NativeNotifications? _notifications;

	/// <summary>Raised for a current, subscribed document while its originating operation is active.</summary>
	/// <remarks>Hosts must marshal to their UI thread and recheck <see cref="IsCurrent"/> before sending. Disposal removes all listeners.</remarks>
	public event Action<NativeEvent>? NativeEventReceived;

	/// <summary>Raised when a document is retired; the host must marshal UI teardown to its owning thread.</summary>
	public event Action? DocumentDisconnected;

	/// <summary>Creates and registers an exclusive registry once from the owning scope's explicitly supplied providers.</summary>
	/// <param name="providers">The scoped provider collection, including any explicitly registered external plugins.</param>
	/// <param name="options">Immutable host policy used for connection capabilities.</param>
	/// <param name="time">Optional clock for document leases.</param>
	/// <param name="viewContext">Optional scoped page/window binding for the bundled bootstrap location.</param>
	/// <param name="notifications">Optional scoped, typed native notification source.</param>
	public HybridAppTargetObject(IEnumerable<IHybridCommandProvider> providers, HybridAppOptions options, TimeProvider? time = null,
		UI.HybridViewContext? viewContext = null, NativeNotifications? notifications = null)
		: this(CreateRegistry(providers), options, time, viewContext)
	{
		_notifications = notifications;
		if (notifications is not null)
		{
			notifications.Received += Notify;
		}
	}


	void Notify(string topic, JsonElement data, Func<bool>? isCurrent)
	{
		lock (_gate)
			if (!_disposed && _session is not null && _registry.HasTopic(topic) && _subscriptions.Contains(topic) &&
				(isCurrent is null || isCurrent()))
				NativeEventReceived?.Invoke(new NativeEvent(1, _session, Guid.NewGuid().ToString("D"), topic, data));
	}

	HybridAppTargetObject(CommandRegistry registry, HybridAppOptions options, TimeProvider? time, UI.HybridViewContext? viewContext)
		: this(registry, CreateReady(registry, options, viewContext), null, time)
	{
	}

	static Func<ReadyResult> CreateReady(CommandRegistry registry, HybridAppOptions options, UI.HybridViewContext? viewContext)
	{
		ArgumentNullException.ThrowIfNull(options);
		return () => new ReadyResult(registry.Names, options.PreferenceNamespace, options.MaximumTextLength,
			registry.InteractiveCommands, InitialLocation: viewContext?.InitialLocation);
	}

	static CommandRegistry CreateRegistry(IEnumerable<IHybridCommandProvider> providers)
	{
		ArgumentNullException.ThrowIfNull(providers);
		var registry = new CommandRegistry();
		foreach (var provider in providers)
			provider.Register(registry);
		return registry;
	}

	/// <summary>Creates a hub after the host explicitly registers its providers.</summary>
	/// <param name="registry">The registry exclusive to this hub.</param>
	/// <param name="ready">Host-supplied connection capabilities; evaluated after control registration.</param>
	/// <param name="send">Delivers events. The host must recheck the session on its UI thread before sending.</param>
	/// <param name="time">Optional clock for document leases.</param>
	public HybridAppTargetObject(CommandRegistry registry, Func<ReadyResult> ready, Action<NativeEvent>? send, TimeProvider? time = null)
	{
		ArgumentNullException.ThrowIfNull(registry);
		ArgumentNullException.ThrowIfNull(ready);
		_registry = registry;
		_ready = ready;
		NativeEventReceived = send;
		_time = time ?? TimeProvider.System;
		RegisterSessionCommands();
		_lease = _time.CreateTimer(_ => ExpireDocument(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
	}

	void RegisterSessionCommands()
	{
		var json = ProtocolJsonContext.Default;
		_registry.Register("core.connect", json.EmptyArgs, json.ReadyResult, (_, _) =>
			Task.FromResult(_ready()));
		_registry.Register("core.subscribe", json.TopicArgs, json.Ack, async (args, op) =>
		{
			ValidateTopic(args.Topic);
			bool start;
			lock (_gate)
			{
				op.Cancellation.ThrowIfCancellationRequested();
				start = _subscriptions.Add(args.Topic);
			}
			try
			{ if (start) await _registry.StartTopicAsync(args.Topic).ConfigureAwait(false); }
			catch { lock (_gate) _subscriptions.Remove(args.Topic); throw; }
			return new Ack(true);
		});
		_registry.Register("core.unsubscribe", json.TopicArgs, json.Ack, (args, op) =>
		{
			ValidateTopic(args.Topic);
			lock (_gate)
			{
				op.Cancellation.ThrowIfCancellationRequested();
				if (_subscriptions.Remove(args.Topic))
					_registry.StopTopic(args.Topic);
			}
			return Task.FromResult(new Ack(true));
		});
		_registry.Register("core.cancel", json.CancelArgs, json.Ack, (args, op) =>
		{
			lock (_gate)
			{
				op.Cancellation.ThrowIfCancellationRequested();
				var found = _operations.TryGetValue(args.RequestId, out var operation);
				operation?.Cancel();
				return Task.FromResult(new Ack(found));
			}
		});
	}

	void ValidateTopic(string topic)
	{
		if (!_registry.HasTopic(topic))
			throw new CommandException("unknown_topic", "This event topic is not registered by the host.");
	}

	/// <inheritdoc />
	public async Task<string> Invoke(string request)
	{
		Request? message = null;
		CancellationTokenSource? cancellation = null;
		CancellationTokenSource? deadline = null;
		try
		{
			if (request.Length > MaximumRequestLength || request.Any(character => character > 127))
				throw new CommandException("invalid_request", "Use an ASCII-escaped JSON request of at most 100000 characters.");
			message = JsonSerializer.Deserialize(request, ProtocolJsonContext.Default.Request)
				?? throw new JsonException();
			if (message.Version != 1 || !Guid.TryParseExact(message.Session, "D", out _) ||
				!Guid.TryParseExact(message.Id, "D", out _) || message.Command.Length is < 1 or > 64)
				throw new CommandException("invalid_request", "Expected protocol version 1 and UUID session/request identities.");
			lock (_gate)
			{
				if (_disposed)
					throw new CommandException("disconnected", "The native WebView has been destroyed.");
				if (message.Command == "core.connect" && _session != message.Session)
				{
					if (_retiredSessions.Contains(message.Session))
						throw new CommandException("stale_session", "This document has been retired. Reload to create a new document identity.");
					if (_retiredSessions.Count >= MaximumRetiredDocuments)
						throw new CommandException("document_limit", "The document lifetime limit was reached. Restart the native app.");
					ResetDocument();
					_session = message.Session;
				}
				if (_session != message.Session)
					throw new CommandException("stale_session", "This document is disconnected. Reload to reconnect.");
				_lastSeen = _time.GetUtcNow();
				if (_operations.Count >= 16)
					throw new CommandException("busy", "At most 16 native calls may be active per document.");
				if (_operations.ContainsKey(message.Id) || !_seen.Add(message.Id))
					throw new CommandException("duplicate_request", "A request identity may only be used once.");
				_seenOrder.Enqueue(message.Id);
				if (_seenOrder.Count > 256)
					_seen.Remove(_seenOrder.Dequeue());
				var interactive = _registry.IsInteractive(message.Command);
				deadline = new CancellationTokenSource(TimeSpan.FromSeconds(interactive ? 300 : 20), _time);
				cancellation = CancellationTokenSource.CreateLinkedTokenSource(_document.Token, deadline.Token);
				_operations.Add(message.Id, cancellation);
				if (interactive)
					_interactiveOperations.Add(message.Id);
			}
			var operation = new Operation(message.Session, message.Id, cancellation.Token,
				(topic, data) => Publish(message.Session, message.Id, topic, data));
			var result = await _registry.DispatchAsync(message.Command, message.Args, operation)
				.WaitAsync(cancellation.Token).ConfigureAwait(false);
			cancellation.Token.ThrowIfCancellationRequested();
			return Encode(message, "success", result, null);
		}
		catch (OperationCanceledException)
		{
			return Encode(message, "cancelled", null, new CommandError("cancelled", "The native operation was cancelled or its document disconnected."));
		}
		catch (CommandException exception)
		{
			LogError(message, exception.Code, exception);
			return Encode(message, "error", null, new CommandError(exception.Code, exception.Message));
		}
		catch (JsonException exception)
		{
			LogError(message, "invalid_request", exception);
			return Encode(message, "error", null, new CommandError("invalid_request", "The request does not match protocol version 1."));
		}
		catch (Exception exception)
		{
			var code = exception is NotSupportedException ? "unsupported" :
				exception is UnauthorizedAccessException ? "permission_denied" : "native_error";
			LogError(message, code, exception);
			return Encode(message, "error", null, new CommandError(code, "The native API failed. Check platform support and OS permissions, then retry."));
		}
		finally
		{
			if (cancellation is not null && message is not null)
			{
				lock (_gate)
				{
					if (_operations.TryGetValue(message.Id, out var current) && ReferenceEquals(current, cancellation))
					{
						_operations.Remove(message.Id);
						_interactiveOperations.Remove(message.Id);
						if (_registry.IsInteractive(message.Command))
							_lastSeen = _time.GetUtcNow();
					}
					cancellation.Dispose();
					deadline?.Dispose();
				}
			}
		}
	}

	static string Encode(Request? request, string status, JsonElement? result, CommandError? error) =>
		JsonSerializer.Serialize(new Response(1, request?.Session ?? "", request?.Id ?? "", status, result, error),
			ProtocolJsonContext.Default.Response);

	static void LogError(Request? request, string code, Exception exception)
	{
		var name = request?.Command;
		if (name is null || name.Length > 64 || name.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '.' and not '_' and not '-'))
			name = "<invalid>";
		Console.WriteLine($"HYBRIDWEBAPP_COMMAND_ERROR command={name} code={code} type={exception.GetType().Name}");
	}

	void Publish(string session, string id, string topic, JsonElement data)
	{
		lock (_gate)
		{
			if (!_disposed && session == _session && _subscriptions.Contains(topic) &&
				_operations.TryGetValue(id, out var operation) && !operation.IsCancellationRequested)
				NativeEventReceived?.Invoke(new NativeEvent(1, session, id, topic, data));
		}
	}

	/// <summary>Rechecks document ownership immediately before host event delivery.</summary>
	/// <param name="session">The event's document identity.</param>
	/// <returns>Whether this document is still connected.</returns>
	public bool IsCurrent(string session)
	{
		lock (_gate)
			return !_disposed && _session == session;
	}

	/// <summary>Receives raw heartbeat/disconnect notices from the owning WebView.</summary>
	/// <param name="value">The raw JSON notice, limited to 256 characters.</param>
	public void ReceiveDocumentMessage(string? value)
	{
		if (value is null || value.Length > 256)
		{
			Console.WriteLine("HYBRIDWEBAPP_DOCUMENT_MESSAGE_REJECTED reason=size");
			return;
		}
		try
		{
			var message = JsonSerializer.Deserialize(value, ProtocolJsonContext.Default.DocumentMessage);
			if (message is null || message.Kind is not "disconnect" and not "heartbeat" || !Guid.TryParseExact(message.Session, "D", out _))
			{
				Console.WriteLine("HYBRIDWEBAPP_DOCUMENT_MESSAGE_REJECTED reason=contract");
				return;
			}
			lock (_gate)
			{
				if (_disposed || message is null || message.Session != _session)
					return;
				if (message.Kind == "disconnect")
					ResetDocument();
				else if (message.Kind == "heartbeat")
					_lastSeen = _time.GetUtcNow();
			}
		}
		catch (JsonException)
		{
			Console.WriteLine("HYBRIDWEBAPP_DOCUMENT_MESSAGE_REJECTED reason=json");
		}
	}

	/// <summary>Expires a document whose heartbeat lease elapsed; also called periodically by the hub.</summary>
	public void ExpireDocument()
	{
		lock (_gate)
		{
			if (!_disposed && _session is not null &&
				!_interactiveOperations.Any(id => _operations.TryGetValue(id, out var op) && !op.IsCancellationRequested) &&
				_time.GetUtcNow() - _lastSeen > TimeSpan.FromSeconds(15))
				ResetDocument();
		}
	}

	/// <summary>Retires the current document and cooperatively cancels all its work.</summary>
	public void Disconnect()
	{
		lock (_gate)
			if (!_disposed)
				ResetDocument();
	}

	void ResetDocument()
	{
		var hadDocument = _session is not null;
		if (_session is not null)
			_retiredSessions.Add(_session);
		_document.Cancel();
		_document.Dispose();
		_document = new CancellationTokenSource();
		_operations.Clear();
		_interactiveOperations.Clear();
		foreach (var topic in _subscriptions)
			_registry.StopTopic(topic);
		_subscriptions.Clear();
		_seen.Clear();
		_seenOrder.Clear();
		_session = null;
		if (hadDocument)
			DocumentDisconnected?.Invoke();
	}

	/// <summary>Retires the document, cancels its work and releases the lease timer.</summary>
	public void Dispose()
	{
		lock (_gate)
		{
			if (_disposed)
				return;
			ResetDocument();
			_disposed = true;
			_document.Dispose();
			_lease.Dispose();
			NativeEventReceived = null;
			DocumentDisconnected = null;
			if (_notifications is not null)
			{
				_notifications.Received -= Notify;
			}
			_notifications = null;
		}
	}
}
