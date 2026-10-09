using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.HybridWebApp;

sealed class LoopbackInspector : IDisposable
{
	const int MaximumResponseBytes = 2 * 1024 * 1024;
	const string Probe = """
		JSON.stringify({
		  href: location.href,
		  readyState: document.readyState,
		  htmlMessage: document.querySelector('#html-message')?.textContent ?? null,
		  typescriptMessage: document.querySelector('#typescript-message')?.textContent ?? null,
		  moduleMessage: document.querySelector('#module-message')?.textContent ?? null,
		  packagedMessage: document.querySelector('#packaged-title')?.textContent ?? null,
		  style: (() => {
		    const element = document.querySelector('#style-target');
		    if (!element) return null;
		    const style = getComputedStyle(element);
		    return {backgroundColor: style.backgroundColor, borderColor: style.borderColor, padding: style.padding};
		  })(),
		  resources: [...new Set([
		    ...performance.getEntriesByType('resource').map(entry => entry.name),
		    ...Array.from(document.querySelectorAll('script[src],link[href],img[src]'), element => element.src || element.href)
		  ])].slice(-64)
		})
		""";
	readonly DevelopmentSession _session;
	readonly HybridWebView _view;
	readonly DevelopmentSettings _settings;
	readonly TcpListener _listener;
	readonly CancellationTokenSource _shutdown = new();
	readonly byte[] _authorization;
	readonly Task _loop;
	bool _disposed;

	public LoopbackInspector(DevelopmentSession session, HybridWebView view, DevelopmentSettings settings)
	{
		_session = session;
		_view = view;
		_settings = settings;
		_authorization = Encoding.ASCII.GetBytes("Bearer " + settings.Token);
		_listener = new TcpListener(IPAddress.Loopback, settings.InspectionPort);
		_listener.Start(4);
		_loop = ListenAsync();
		Console.WriteLine($"HYBRIDWEBAPP_INSPECT listening=127.0.0.1:{settings.InspectionPort} endpoints=/state,/source,/probe,/snapshot");
	}

	async Task ListenAsync()
	{
		try
		{
			while (!_shutdown.IsCancellationRequested)
			{
				using var client = await _listener.AcceptTcpClientAsync(_shutdown.Token).ConfigureAwait(false);
				using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token);
				timeout.CancelAfter(TimeSpan.FromSeconds(10));
				try
				{
					await HandleAsync(client.GetStream(), timeout.Token).ConfigureAwait(false);
				}
				catch (Exception exception) when (exception is IOException or OperationCanceledException or SocketException)
				{
					if (!_shutdown.IsCancellationRequested)
						Console.WriteLine($"HYBRIDWEBAPP_INSPECT_REQUEST_FAILED {exception.GetType().Name}");
				}
			}
		}
		catch (Exception exception) when (_shutdown.IsCancellationRequested && exception is OperationCanceledException or SocketException or ObjectDisposedException)
		{
		}
		catch (Exception exception)
		{
			Console.WriteLine($"HYBRIDWEBAPP_INSPECT_FAILED {exception.Message}");
		}
	}

	async Task HandleAsync(NetworkStream stream, CancellationToken cancellationToken)
	{
		var buffer = new byte[8192];
		var length = 0;
		while (true)
		{
			if (length == buffer.Length)
			{
				await WriteAsync(stream, 413, "text/plain", Encoding.UTF8.GetBytes("Request headers too large."), cancellationToken).ConfigureAwait(false);
				return;
			}
			var read = await stream.ReadAsync(buffer.AsMemory(length), cancellationToken).ConfigureAwait(false);
			if (read == 0)
				return;
			length += read;
			if (Encoding.ASCII.GetString(buffer, 0, length).Contains("\r\n\r\n", StringComparison.Ordinal))
				break;
		}

		var lines = Encoding.ASCII.GetString(buffer, 0, length).Split("\r\n", StringSplitOptions.None);
		var request = lines[0].Split(' ');
		var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (var line in lines.Skip(1))
		{
			if (line.Length == 0)
				break;
			var separator = line.IndexOf(":", StringComparison.Ordinal);
			if (separator <= 0 || !headers.TryAdd(line[..separator], line[(separator + 1)..].Trim()))
			{
				await WriteAsync(stream, 400, "text/plain", Encoding.UTF8.GetBytes("Invalid request headers."), cancellationToken).ConfigureAwait(false);
				return;
			}
		}

		var authenticated = headers.TryGetValue("Authorization", out var authorization) &&
			CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(authorization), _authorization);
		if (!authenticated)
		{
			await WriteAsync(stream, 401, "text/plain", Encoding.UTF8.GetBytes("Bearer token required."), cancellationToken).ConfigureAwait(false);
			return;
		}
		if (request.Length != 3 || request[0] != "GET" || request[2] != "HTTP/1.1" ||
			!headers.TryGetValue("Host", out var host) || host != $"127.0.0.1:{_settings.InspectionPort}" ||
			headers.ContainsKey("Transfer-Encoding") ||
			(headers.TryGetValue("Content-Length", out var contentLength) && contentLength != "0"))
		{
			await WriteAsync(stream, 400, "text/plain", Encoding.UTF8.GetBytes("Only body-free loopback HTTP/1.1 GET requests are supported."), cancellationToken).ConfigureAwait(false);
			return;
		}

		try
		{
			var response = await MainThread.InvokeOnMainThreadAsync(() => ProbeAsync(request[1]))
				.WaitAsync(TimeSpan.FromSeconds(8), cancellationToken).ConfigureAwait(false);
			if (response.Bytes.Length > MaximumResponseBytes)
			{
				await WriteAsync(stream, 413, "text/plain", Encoding.UTF8.GetBytes("Inspection response too large."), cancellationToken).ConfigureAwait(false);
				return;
			}
			await WriteAsync(stream, response.Status, response.Mime, response.Bytes, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception exception) when (exception is not OperationCanceledException)
		{
			Console.WriteLine($"HYBRIDWEBAPP_INSPECT_PROBE_FAILED {exception.Message}");
			await WriteAsync(stream, 503, "text/plain", Encoding.UTF8.GetBytes("Native WebView probe unavailable."), cancellationToken).ConfigureAwait(false);
		}
	}

	async Task<(int Status, string Mime, byte[] Bytes)> ProbeAsync(string path)
	{
		if (_session.IsDisposed || !_session.Platform.IsAttached(_view))
			throw new InvalidOperationException("The native WebView is not attached.");

		if (path == "/state")
			return (200, "application/json", State());
		if (path == "/source")
		{
			var source = await _view.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
			return (200, "text/plain; charset=utf-8", Encoding.UTF8.GetBytes(DecodeJavaScriptString(source)));
		}
		if (path == "/probe")
		{
			var probe = await _view.EvaluateJavaScriptAsync(Probe);
			var json = DecodeJavaScriptString(probe);
			using var document = JsonDocument.Parse(json);
			return (200, "application/json", Encoding.UTF8.GetBytes(json));
		}
		if (path == "/snapshot")
		{
			return (200, "image/png", await _session.Platform.SnapshotAsync());
		}
		return (404, "text/plain", Encoding.UTF8.GetBytes("Unknown read-only probe."));
	}

	static string DecodeJavaScriptString(string? value)
	{
		if (value is null)
			throw new InvalidOperationException("The JavaScript probe returned no result.");
		// Controls strips JSON string quotes on all platforms; retained escapes need decoding.
		using var document = JsonDocument.Parse("\"" + value + "\"");
		return document.RootElement.GetString()!;
	}

	byte[] State()
	{
		using var stream = new MemoryStream();
		using (var json = new Utf8JsonWriter(stream))
		{
			json.WriteStartObject();
			json.WriteNumber("pid", Environment.ProcessId);
			json.WriteString("startupId", MauiProgram.StartupId);
			json.WriteString("startedAt", MauiProgram.StartedAt);
			json.WriteString("uri", _session.Platform.Uri);
			json.WriteString("mode", _session.Mode);
			var metrics = _session.GetMetrics();
			json.WriteNumber("requests", metrics.Total);
			json.WriteNumber("active", metrics.Active);
			json.WriteNumber("canceled", metrics.Canceled);
			json.WriteNumber("failed", metrics.Failed);
			json.WriteStartArray("responses");
			foreach (var response in metrics.Requests)
			{
				json.WriteStartObject();
				json.WriteString("pathAndQuery", response.PathAndQuery);
				json.WriteNumber("status", response.Status);
				json.WriteString("mime", response.Mime);
				json.WriteEndObject();
			}
			json.WriteEndArray();
			json.WriteEndObject();
		}
		return stream.ToArray();
	}

	static async Task WriteAsync(NetworkStream stream, int status, string mime, byte[] bytes, CancellationToken cancellationToken)
	{
		var header = Encoding.ASCII.GetBytes($"HTTP/1.1 {status} Inspection\r\nContent-Type: {mime}\r\nContent-Length: {bytes.Length}\r\nCache-Control: no-store\r\nX-Content-Type-Options: nosniff\r\nContent-Security-Policy: default-src 'none'; frame-ancestors 'none'\r\nConnection: close\r\n\r\n");
		await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
		await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
	}

	public void Dispose()
	{
		if (_disposed)
			return;
		_disposed = true;
		_shutdown.Cancel();
		_listener.Stop();
		_ = _loop.ContinueWith(_ => _shutdown.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	}
}
