using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler
	{
		internal const string InvokeDotNetBinaryPath = "__hwvInvokeDotNetBinary";
		internal const string BinaryTokenHeader = "X-Maui-Binary-Token";
		internal const string BinaryTokenValue = "HybridWebView/1";
		internal const string BinarySessionPlaceholder = "__MAUI_BINARY_DOCUMENT_SESSION__";
		readonly object _binaryGate = new();
		HybridWebViewBinaryInvocationManager? _binaryInvocations;

		internal string BinaryDocumentSession
		{
			get { lock (_binaryGate) return _binaryInvocations?.Session ?? ""; }
		}

		internal void ResetBinaryInvocation(bool disconnected = false)
		{
			HybridWebViewBinaryInvocationManager? manager;
			lock (_binaryGate)
				manager = _binaryInvocations;
			manager?.Reset(disconnected ? null : (VirtualView as IHybridWebViewBinary)?.BinaryInvoker);
		}

		internal Stream? GetHybridScriptStream()
		{
			var script = GetEmbeddedStream(HybridWebViewDotJsPath);
			if (script is null)
				return null;
			string session = "";
			lock (_binaryGate)
			{
				if ((VirtualView as IHybridWebViewBinary)?.BinaryInvoker is { } invoker)
				{
					_binaryInvocations ??= new HybridWebViewBinaryInvocationManager(invoker, logger: MauiContext?.CreateLogger<HybridWebViewHandler>());
					session = _binaryInvocations.Session;
				}
			}
			if (session.Length == 0)
				return script;
			using var reader = new StreamReader(script);
			var content = reader.ReadToEnd();
#if NETSTANDARD2_0
			content = content.Replace(BinarySessionPlaceholder, session);
#else
			content = content.Replace(BinarySessionPlaceholder, session, StringComparison.Ordinal);
#endif
			return new MemoryStream(Encoding.UTF8.GetBytes(content), writable: false);
		}

		internal static bool IsBinaryRequest(Uri uri) =>
			AppOriginUri.IsBaseOf(uri) && uri.AbsolutePath == "/" + InvokeDotNetBinaryPath;

		internal static bool IsBinaryAppDocument(string? url) =>
			Uri.TryCreate(url, UriKind.Absolute, out var uri) && AppOriginUri.IsBaseOf(uri);

		// Android cannot expose an asynchronous HTTP status/header. All platforms therefore use
		// a one-byte discriminator: 0 + raw bytes, or 1 + bounded structured UTF-8 failure.
		internal async Task<byte[]> InvokeDotNetBinaryAsync(
			IEnumerable<KeyValuePair<string, string>>? headers,
			string? httpMethod,
			bool trustedDocument,
			bool fragmented,
			Stream? body = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				if (!trustedDocument || !string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase))
					throw new InvalidOperationException("Binary requests require a local document and POST.");
				var values = ValidateBinaryTransportHeaders(headers);
				var session = Required(values, "X-Maui-Binary-Session", 32);
				var id = ReadInteger(values, "X-Maui-Binary-Id");
				if (id == 0)
					throw new InvalidOperationException("Binary request ids must be positive.");
				var operation = Required(values, "X-Maui-Binary-Operation", 16);
				HybridWebViewBinaryInvocationManager? manager;
				lock (_binaryGate)
					manager = _binaryInvocations;
				if (manager is null)
					throw new InvalidOperationException("Binary invocation is not configured.");
				cancellationToken.ThrowIfCancellationRequested();
				switch (operation)
				{
					case "begin":
						var encoded = Required(values, "X-Maui-Binary-Metadata", 16384);
						var metadata = Uri.UnescapeDataString(encoded);
						if (metadata.Length > 8192)
							throw new ArgumentException("Binary metadata is too large.");
						var data = JsonSerializer.Deserialize(metadata, HybridWebViewHandlerJsonContext.Default.BinaryBeginData)
							?? throw new ArgumentException("Binary metadata is missing.");
						manager.Begin(session, id, data.MethodName ?? "",
							data.Length ?? throw new ArgumentException("Binary payload length is missing."), data.Headers, fragmented);
						return new byte[] { 0 };
					case "chunk":
						if (!fragmented)
							throw new InvalidOperationException("This platform does not accept encoded binary fragments.");
						manager.Append(session, id, ReadInteger(values, "X-Maui-Binary-Index"),
							Required(values, "X-Maui-Binary-Fragment", HybridWebViewBinaryInvocationManager.FragmentLength / 3 * 4));
						return new byte[] { 0 };
					case "invoke":
						var bytes = await manager.InvokeAsync(session, id, body, cancellationToken).ConfigureAwait(false);
						var response = new byte[bytes.Length + 1];
						Buffer.BlockCopy(bytes, 0, response, 1, bytes.Length);
						return response;
					case "cancel":
						manager.Cancel(session, id);
						return new byte[] { 0 };
					default:
						throw new InvalidOperationException("Unknown binary operation.");
				}
			}
			catch (Exception ex)
			{
				var error = new BinaryError
				{
					IsError = true,
					ErrorType = ex.GetType().Name,
					ErrorMessage = ex.Message.Length > 512 ? ex.Message.Substring(0, 512) : ex.Message
				};
				var json = JsonSerializer.SerializeToUtf8Bytes(error, HybridWebViewHandlerJsonContext.Default.BinaryError);
				var response = new byte[json.Length + 1];
				response[0] = 1;
				Buffer.BlockCopy(json, 0, response, 1, json.Length);
				return response;
			}
		}

		static Dictionary<string, string> ValidateBinaryTransportHeaders(IEnumerable<KeyValuePair<string, string>>? headers)
		{
			var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (headers is not null)
				foreach (var header in headers)
					if (header.Key.StartsWith("X-Maui-Binary-", StringComparison.OrdinalIgnoreCase) ||
						header.Key.Equals("Origin", StringComparison.OrdinalIgnoreCase) ||
						header.Key.Equals("Referer", StringComparison.OrdinalIgnoreCase) ||
						header.Key.Equals("Sec-Fetch-Dest", StringComparison.OrdinalIgnoreCase))
					{
						if (values.Count >= 12 || header.Value.Length > 16384 || values.ContainsKey(header.Key))
							throw new ArgumentException("Invalid binary transport headers.");
						values.Add(header.Key, header.Value);
					}
			if (!values.TryGetValue(BinaryTokenHeader, out var marker) || marker != BinaryTokenValue)
				throw new ArgumentException("Binary protocol marker is missing.");
			if (values.TryGetValue("Origin", out var origin))
			{
				if (origin != AppOrigin.TrimEnd('/'))
					throw new ArgumentException("Binary request origin is not trusted.");
			}
			else if (!values.TryGetValue("Referer", out var referer) || !IsBinaryAppDocument(referer))
				throw new ArgumentException("Binary request origin is missing.");
			if (values.TryGetValue("Sec-Fetch-Dest", out var destination) && destination != "empty")
				throw new ArgumentException("Binary endpoints only accept fetch requests.");
			return values;
		}

		static string Required(Dictionary<string, string> values, string name, int maximumLength)
		{
			if (!values.TryGetValue(name, out var value) || value.Length == 0 || value.Length > maximumLength)
				throw new ArgumentException($"Invalid {name} header.");
			return value;
		}

		static int ReadInteger(Dictionary<string, string> values, string name)
		{
			if (!int.TryParse(Required(values, name, 10), NumberStyles.None, CultureInfo.InvariantCulture, out var value))
				throw new ArgumentException($"Invalid {name} header.");
			return value;
		}

		private sealed class BinaryBeginData
		{
			public string? MethodName { get; set; }
			public int? Length { get; set; }
			public Dictionary<string, string>? Headers { get; set; }
		}

		private sealed class BinaryError
		{
			public bool IsError { get; set; }
			public string? ErrorType { get; set; }
			public string? ErrorMessage { get; set; }
		}
	}
}
