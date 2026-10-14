using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Handlers
{
	// One manager per handler. Canceled host calls retain their slot until they actually finish.
	internal sealed class HybridWebViewBinaryInvocationManager
	{
		internal const int MaximumPayloadLength = 4 * 1024 * 1024;
		internal const int MaximumConcurrentInvocations = 4;
		internal const int FragmentLength = 12 * 1024;
		internal const int TimeoutMilliseconds = 30000;
		readonly object _gate = new();
		readonly Dictionary<int, Transfer> _transfers = new();
		readonly HashSet<Transfer> _slots = new();
		readonly int _timeoutMilliseconds;
		readonly ILogger? _logger;
		HybridWebViewBinaryInvoker? _invoker;
		string _session = "";
		int _lastId;

		internal HybridWebViewBinaryInvocationManager(HybridWebViewBinaryInvoker invoker, int timeoutMilliseconds = TimeoutMilliseconds, ILogger? logger = null)
		{
			if (timeoutMilliseconds <= 0)
				throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));
			_timeoutMilliseconds = timeoutMilliseconds;
			_logger = logger;
			Reset(invoker);
		}

		internal string Session
		{
			get { lock (_gate) return _session; }
		}

		internal void Reset(HybridWebViewBinaryInvoker? invoker)
		{
			Transfer[] previous;
			lock (_gate)
			{
				previous = new Transfer[_slots.Count];
				_slots.CopyTo(previous);
				_transfers.Clear();
				_invoker = invoker;
				_session = invoker is null ? "" : Guid.NewGuid().ToString("N");
				_lastId = 0;
				foreach (var transfer in previous)
				{
					transfer.Payload = Array.Empty<byte>();
					if (!transfer.Invoking)
						_slots.Remove(transfer);
				}
			}
			foreach (var transfer in previous)
				Stop(transfer);
		}

		internal void Begin(string session, int id, string method, int length, IDictionary<string, string>? headers, bool fragmented)
		{
			var metadata = ValidateMetadata(method, headers);
			if (length < 0 || length > MaximumPayloadLength)
				throw new ArgumentOutOfRangeException(nameof(length), "Binary payloads are limited to 4 MiB.");
			lock (_gate)
			{
				CheckSession(session);
				if (id <= _lastId)
					throw new InvalidOperationException("Duplicate or out-of-order binary request id.");
				if (_slots.Count >= MaximumConcurrentInvocations)
					throw new InvalidOperationException("At most four binary invocations may be active.");
				_lastId = id;
				var transfer = new Transfer(id, method, length, metadata, fragmented);
				_transfers.Add(id, transfer);
				_slots.Add(transfer);
				transfer.Timer = new Timer(_ => Expire(transfer), null, _timeoutMilliseconds, Timeout.Infinite);
			}
		}

		internal void Append(string session, int id, int index, string encoded)
		{
			Transfer? invalidTransfer = null;
			try
			{
				lock (_gate)
				{
					var transfer = Find(session, id);
					try
					{
						if (!transfer.Fragmented || transfer.Invoking || index != transfer.NextFragment ||
							encoded.Length == 0 || encoded.Length > FragmentLength / 3 * 4)
							throw new InvalidOperationException("Invalid or out-of-order binary fragment.");
						var bytes = Convert.FromBase64String(encoded);
						var expectedLength = Math.Min(FragmentLength, transfer.Length - transfer.Offset);
						if (expectedLength == 0 || bytes.Length != expectedLength || Convert.ToBase64String(bytes) != encoded)
							throw new InvalidOperationException("Malformed binary fragment.");
						Buffer.BlockCopy(bytes, 0, transfer.Payload, transfer.Offset, bytes.Length);
						transfer.Offset += bytes.Length;
						transfer.NextFragment++;
					}
					catch
					{
						if (!transfer.Invoking)
							Remove(transfer);
						invalidTransfer = transfer;
						throw;
					}
				}
			}
			finally
			{
				if (invalidTransfer is not null)
					Stop(invalidTransfer);
			}
		}

		internal async Task<byte[]> InvokeAsync(string session, int id, Stream? body, CancellationToken cancellationToken)
		{
			Transfer transfer;
			HybridWebViewBinaryInvoker invoker;
			byte[] uploadedPayload;
			lock (_gate)
			{
				transfer = Find(session, id);
				if (transfer.Invoking)
					throw new InvalidOperationException("Binary request has already been invoked.");
				if (transfer.Fragmented && transfer.Offset != transfer.Length)
				{
					Remove(transfer);
					Stop(transfer);
					throw new InvalidOperationException("Binary upload is incomplete.");
				}
				transfer.Invoking = true;
				invoker = _invoker!;
				uploadedPayload = transfer.Payload;
			}

			using var linked = CancellationTokenSource.CreateLinkedTokenSource(transfer.Cancellation.Token, cancellationToken);
			try
			{
				var token = linked.Token;
				token.ThrowIfCancellationRequested();
				var payload = uploadedPayload;
				if (!transfer.Fragmented)
				{
					payload = new byte[transfer.Length];
					var offset = 0;
					if (body is null && payload.Length != 0)
						throw new InvalidOperationException("Binary request body is missing.");
					if (body is not null)
					{
						while (offset < payload.Length)
						{
							var read = await body.ReadAsync(payload, offset, payload.Length - offset, token).ConfigureAwait(false);
							if (read == 0)
								throw new InvalidOperationException("Binary request body is truncated.");
							offset += read;
						}
						if (await body.ReadAsync(new byte[1], 0, 1, token).ConfigureAwait(false) != 0)
							throw new InvalidOperationException("Binary request body exceeds its declared length.");
					}
				}
				token.ThrowIfCancellationRequested();
				var result = await invoker.InvokeAsync(new HybridWebViewBinaryRequest(transfer.Method, payload, transfer.Headers), token).ConfigureAwait(false);
				token.ThrowIfCancellationRequested();
				if (result is null || result.Length > MaximumPayloadLength)
					throw new InvalidOperationException("Binary results must be non-null and no larger than 4 MiB.");
				lock (_gate)
					CheckSession(session);
				return result;
			}
			finally
			{
				lock (_gate)
					Remove(transfer);
				transfer.Timer?.Dispose();
			}
		}

		internal void Cancel(string session, int id)
		{
			Transfer transfer;
			lock (_gate)
			{
				transfer = Find(session, id);
				if (transfer.Cancellation.IsCancellationRequested)
					throw new InvalidOperationException("Binary request has already been canceled.");
				transfer.Payload = Array.Empty<byte>();
				if (!transfer.Invoking)
					Remove(transfer);
			}
			Stop(transfer);
		}

		void Expire(Transfer transfer)
		{
			lock (_gate)
			{
				if (!_slots.Contains(transfer))
					return;
				transfer.Payload = Array.Empty<byte>();
				if (!transfer.Invoking)
					Remove(transfer);
			}
			Stop(transfer);
		}

		void Stop(Transfer transfer)
		{
			transfer.Timer?.Dispose();
			// User cancellation callbacks must not unwind a platform callback or strand other transfers.
			try
			{ transfer.Cancellation.Cancel(); }
			catch (AggregateException exception)
			{
				_logger?.LogError(exception, "A binary invocation cancellation callback failed.");
			}
		}

		void Remove(Transfer transfer)
		{
			if (_transfers.TryGetValue(transfer.Id, out var current) && ReferenceEquals(current, transfer))
				_transfers.Remove(transfer.Id);
			_slots.Remove(transfer);
			transfer.Payload = Array.Empty<byte>();
		}

		void CheckSession(string session)
		{
			if (_invoker is null || session.Length != 32 || session != _session)
				throw new InvalidOperationException("Binary invocation is disabled, disconnected, or belongs to a stale document.");
		}

		Transfer Find(string session, int id)
		{
			CheckSession(session);
			if (!_transfers.TryGetValue(id, out var transfer))
				throw new InvalidOperationException("Unknown binary request id.");
			return transfer;
		}

		internal static IReadOnlyDictionary<string, string> ValidateMetadata(string method, IDictionary<string, string>? headers)
		{
			if (string.IsNullOrEmpty(method) || method.Length > 128)
				throw new ArgumentException("Binary command names must contain 1 to 128 ASCII characters.");
			foreach (var c in method)
				if (!IsAlphaNumeric(c) && c != '_' && c != '-' && c != '.' && c != ':')
					throw new ArgumentException("Invalid binary command name.");
			var copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (headers is not null)
			{
				if (headers.Count > 16)
					throw new ArgumentException("At most 16 binary metadata headers are allowed.");
				foreach (var pair in headers)
				{
					if (string.IsNullOrEmpty(pair.Key) || pair.Key.Length > 64 || pair.Value is null || pair.Value.Length > 256)
						throw new ArgumentException("Invalid binary metadata header length.");
					foreach (var c in pair.Key)
						if (!IsAlphaNumeric(c) && c != '-')
							throw new ArgumentException("Invalid binary metadata header name.");
					foreach (var c in pair.Value)
						if (c < 32 || c > 126)
							throw new ArgumentException("Binary metadata values must be printable ASCII.");
					copy.Add(pair.Key, pair.Value);
				}
			}
			return new ReadOnlyDictionary<string, string>(copy);
		}

		static bool IsAlphaNumeric(char c) => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9';

		sealed class Transfer
		{
			internal readonly int Id;
			internal readonly string Method;
			internal readonly int Length;
			internal readonly IReadOnlyDictionary<string, string> Headers;
			internal readonly bool Fragmented;
			internal readonly CancellationTokenSource Cancellation = new();
			internal Timer? Timer;
			internal byte[] Payload;
			internal int Offset;
			internal int NextFragment;
			internal bool Invoking;

			internal Transfer(int id, string method, int length, IReadOnlyDictionary<string, string> headers, bool fragmented)
			{
				Id = id;
				Method = method;
				Length = length;
				Headers = headers;
				Fragmented = fragmented;
				Payload = fragmented ? new byte[length] : Array.Empty<byte>();
			}
		}
	}
}
