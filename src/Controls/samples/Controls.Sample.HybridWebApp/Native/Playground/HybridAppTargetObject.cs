using System.Text.Json;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class HybridAppTargetObject : IHybridAppTarget, IDisposable
{
	const int MaximumRequestLength = 100000;
	const int MaximumRetiredDocuments = 4096;
	readonly object _gate = new();
	readonly CommandRegistry _registry = new();
	readonly Dictionary<string, CancellationTokenSource> _operations = new(StringComparer.Ordinal);
	readonly HashSet<string> _subscriptions = new(StringComparer.Ordinal);
	readonly HashSet<string> _seen = new(StringComparer.Ordinal);
	readonly HashSet<string> _retiredSessions = new(StringComparer.Ordinal);
	readonly Queue<string> _seenOrder = new();
	readonly Timer _lease;
	readonly TimeProvider _time;
	readonly Action<NativeEvent> _send;
	CancellationTokenSource _document = new();
	string? _session;
	DateTimeOffset _lastSeen;
	bool _disposed;

	public HybridAppTargetObject(IPlaygroundPlatform platform, Action<NativeEvent> send, TimeProvider? time = null)
	{
		_send = send;
		_time = time ?? TimeProvider.System;
		EssentialsProvider.Register(_registry, platform);
		new SampleCommands().Register(_registry);
		RegisterSessionCommands();
		_lease = new Timer(_ => ExpireDocument(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
	}

	void RegisterSessionCommands()
	{
		var json = PlaygroundJsonContext.Default;
		_registry.Register("core.connect", json.EmptyArgs, json.ReadyResult, (_, _) =>
			Task.FromResult(new ReadyResult(_registry.Names, EssentialsProvider.PreferenceNamespace, EssentialsProvider.MaximumTextLength)));
		_registry.Register("core.subscribe", json.TopicArgs, json.Ack, (args, op) =>
		{
			ValidateTopic(args.Topic);
			lock (_gate)
			{
				op.Cancellation.ThrowIfCancellationRequested();
				_subscriptions.Add(args.Topic);
			}
			return Task.FromResult(new Ack(true));
		});
		_registry.Register("core.unsubscribe", json.TopicArgs, json.Ack, (args, op) =>
		{
			ValidateTopic(args.Topic);
			lock (_gate)
			{
				op.Cancellation.ThrowIfCancellationRequested();
				_subscriptions.Remove(args.Topic);
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

	static void ValidateTopic(string topic)
	{
		if (topic is not "service.changed" and not "operation.progress")
			throw new CommandException("unknown_topic", "Only service.changed and operation.progress are available.");
	}

	public async Task<string> Invoke(string request)
	{
		Request? message = null;
		CancellationTokenSource? cancellation = null;
		try
		{
			if (request.Length > MaximumRequestLength || request.Any(character => character > 127))
				throw new CommandException("invalid_request", "Use an ASCII-escaped JSON request of at most 100000 characters.");
			message = JsonSerializer.Deserialize(request, PlaygroundJsonContext.Default.Request)
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
				cancellation = CancellationTokenSource.CreateLinkedTokenSource(_document.Token);
				cancellation.CancelAfter(TimeSpan.FromSeconds(20));
				_operations.Add(message.Id, cancellation);
			}
			var operation = new Operation(message.Session, message.Id, cancellation.Token,
				(topic, data) => Publish(message.Session, message.Id, topic, data));
			var result = await _registry.DispatchAsync(message.Command, message.Args, operation).ConfigureAwait(false);
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
						_operations.Remove(message.Id);
					cancellation.Dispose();
				}
			}
		}
	}

	static string Encode(Request? request, string status, JsonElement? result, CommandError? error) =>
		JsonSerializer.Serialize(new Response(1, request?.Session ?? "", request?.Id ?? "", status, result, error),
			PlaygroundJsonContext.Default.Response);

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
				_send(new NativeEvent(1, session, id, topic, data));
		}
	}

	public bool IsCurrent(string session)
	{
		lock (_gate)
			return !_disposed && _session == session;
	}

	public void ReceiveDocumentMessage(string? value)
	{
		if (value is null || value.Length > 256)
		{
			Console.WriteLine("HYBRIDWEBAPP_DOCUMENT_MESSAGE_REJECTED reason=size");
			return;
		}
		try
		{
			var message = JsonSerializer.Deserialize(value, PlaygroundJsonContext.Default.DocumentMessage);
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

	internal void ExpireDocument()
	{
		lock (_gate)
		{
			if (!_disposed && _session is not null && _time.GetUtcNow() - _lastSeen > TimeSpan.FromSeconds(15))
				ResetDocument();
		}
	}

	public void Disconnect()
	{
		lock (_gate)
			if (!_disposed)
				ResetDocument();
	}

	void ResetDocument()
	{
		if (_session is not null)
			_retiredSessions.Add(_session);
		_document.Cancel();
		_document.Dispose();
		_document = new CancellationTokenSource();
		_operations.Clear();
		_subscriptions.Clear();
		_seen.Clear();
		_seenOrder.Clear();
		_session = null;
	}

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
		}
	}
}
