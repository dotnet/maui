using System.Net;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.HybridWebApp;

sealed class DevelopmentSession : IDisposable
{
	const int MaximumAssetBytes = 8 * 1024 * 1024;
	readonly HybridWebView _view;
	readonly DevelopmentSettings _settings;
	readonly CancellationTokenSource _shutdown = new();
	readonly HttpClient _client = new(new HttpClientHandler
	{
		AllowAutoRedirect = false,
		AutomaticDecompression = DecompressionMethods.All,
		UseCookies = false,
		UseProxy = false
	})
	{ Timeout = TimeSpan.FromSeconds(10) };
	readonly object _gate = new();
	readonly Queue<RequestRecord> _requests = new();
	readonly LoopbackInspector? _inspector;
	readonly DevelopmentPlatform _platform;
	CancellationTokenSource _document = new();
	long _total;
	int _active;
	int _canceled;
	int _failed;
	volatile bool _disposed;

	internal DevelopmentPlatform Platform => _platform;
	internal bool IsDisposed => _disposed;
	internal string Mode => _settings.Upstream is null ? "packaged" : "vite";

	public DevelopmentSession(HybridWebView view, DevelopmentSettings settings)
	{
		_view = view;
		_settings = settings;
		_platform = new DevelopmentPlatform(StartingDocument);
		view.WebResourceRequested += ResourceRequested;
		view.WebViewInitialized += Initialized;
		if (settings.Token is not null)
			_inspector = new LoopbackInspector(this, view, settings);
	}

	void Initialized(object? sender, WebViewInitializedEventArgs e)
	{
		_platform.Attach(e, _settings.Upstream is not null);
	}

	void StartingDocument()
	{
		lock (_gate)
		{
			if (_disposed)
				return;
			_document.Cancel();
			_document.Dispose();
			_document = new CancellationTokenSource();
		}
	}

	void ResourceRequested(object? sender, WebViewWebResourceRequestedEventArgs e)
	{
		if (_disposed || _settings.Upstream is null || e.Uri.Scheme != DevelopmentPlatform.AppScheme || e.Uri.Host != "0.0.0.1" ||
			!e.Uri.IsDefaultPort || e.Uri.UserInfo.Length != 0)
			return;

		var path = Uri.UnescapeDataString(e.Uri.AbsolutePath);
		if (path is "/_framework/hybridwebview.js" or "/__hwvInvokeDotNet" or "/__hwvSendMessage")
		{
			Console.WriteLine($"HYBRIDWEBAPP_NATIVE_ROUTE {e.Uri.PathAndQuery}");
			return;
		}

#if ANDROID
		// ShouldInterceptRequest requires response metadata before its worker callback returns.
		if (MainThread.IsMainThread)
			throw new InvalidOperationException("Development interception must not block the Android UI thread.");
		if (e.PlatformArgs?.Request.IsForMainFrame == true)
			StartingDocument();
#endif
		CancellationToken documentToken;
		CancellationTokenSource cancellation;
		lock (_gate)
		{
			if (_disposed)
				return;
			documentToken = _document.Token;
			cancellation = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token, documentToken);
			_total++;
			_active++;
		}
		e.Handled = true;
		cancellation.CancelAfter(TimeSpan.FromSeconds(10));
#if WINDOWS
		var deferral = e.PlatformArgs!.RequestEventArgs.GetDeferral();
		_ = RespondDeferredAsync();
		async Task RespondDeferredAsync()
		{
			try
			{
				await RespondAsync(e, cancellation, documentToken).ConfigureAwait(false);
			}
			finally
			{
				await MainThread.InvokeOnMainThreadAsync(() =>
				{
					deferral.Complete();
					deferral.Dispose();
				}).ConfigureAwait(false);
			}
		}
#elif ANDROID
		RespondAsync(e, cancellation, documentToken).GetAwaiter().GetResult();
#else
		_ = RespondAsync(e, cancellation, documentToken);
#endif
	}

	async Task RespondAsync(WebViewWebResourceRequestedEventArgs e, CancellationTokenSource cancellation, CancellationToken documentToken)
	{
		try
		{
			if (e.Method is not "GET" and not "HEAD")
			{
				await SendAsync(e, 405, "Method Not Allowed", "text/plain", Encoding.UTF8.GetBytes("Only GET and HEAD development assets are supported."), cancellation.Token);
				return;
			}

			var pathAndQuery = e.Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
			var uri = new Uri(_settings.Upstream!.GetLeftPart(UriPartial.Authority) + pathAndQuery);
			using var request = new HttpRequestMessage(new HttpMethod(e.Method), uri);
			request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
			request.Headers.TryAddWithoutValidation("Accept-Encoding", "identity");
			using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation.Token).ConfigureAwait(false);
			if ((int)response.StatusCode is >= 300 and < 400)
			{
				lock (_gate)
					_failed++;
				Console.WriteLine($"HYBRIDWEBAPP_PROXY_REDIRECT_REJECTED {e.Uri.PathAndQuery}");
				await SendErrorAsync(e, 502, "Bad Gateway", "Vite asset redirects are not supported by this development sample.").ConfigureAwait(false);
				return;
			}
			var bytes = e.Method == "HEAD" ? Array.Empty<byte>() : await ReadBoundedAsync(response.Content, cancellation.Token).ConfigureAwait(false);
			var mime = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
			await SendAsync(e, (int)response.StatusCode, response.ReasonPhrase ?? "", mime, bytes, cancellation.Token,
				e.Method == "HEAD" ? response.Content.Headers.ContentLength : bytes.Length).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			lock (_gate)
				_canceled++;
			Console.WriteLine($"HYBRIDWEBAPP_PROXY_CANCELED {e.Uri.PathAndQuery}");
#if ANDROID
			// Even an obsolete document's synchronous callback must return response metadata.
			await SendErrorAsync(e, 504, "Gateway Timeout", "Vite asset fetch was canceled or timed out.").ConfigureAwait(false);
#else
			if (!_disposed && !documentToken.IsCancellationRequested)
				await SendErrorAsync(e, 504, "Gateway Timeout", "Vite asset fetch timed out.").ConfigureAwait(false);
#endif
		}
		catch (Exception exception)
		{
			lock (_gate)
				_failed++;
			Console.WriteLine($"HYBRIDWEBAPP_PROXY_ERROR {e.Uri.PathAndQuery} {exception.Message}");
			await SendErrorAsync(e, 502, "Bad Gateway", "Vite asset fetch failed; packaged content was not substituted.").ConfigureAwait(false);
		}
		finally
		{
			lock (_gate)
				_active--;
			cancellation.Dispose();
		}
	}

	static async Task<byte[]> ReadBoundedAsync(HttpContent content, CancellationToken cancellationToken)
	{
		if (content.Headers.ContentLength > MaximumAssetBytes)
			throw new InvalidDataException("Development asset exceeds the 8 MiB limit.");
		using var source = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
		using var destination = new MemoryStream();
		var buffer = new byte[16 * 1024];
		int count;
		while ((count = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
		{
			if (destination.Length + count > MaximumAssetBytes)
				throw new InvalidDataException("Development asset exceeds the 8 MiB limit.");
			destination.Write(buffer, 0, count);
		}
		return destination.ToArray();
	}

	async Task SendErrorAsync(WebViewWebResourceRequestedEventArgs e, int status, string reason, string message)
	{
		try
		{
			await SendAsync(e, status, reason, "text/plain; charset=utf-8", Encoding.UTF8.GetBytes(message), CancellationToken.None).ConfigureAwait(false);
		}
		catch (Exception exception)
		{
			Console.WriteLine($"HYBRIDWEBAPP_PROXY_RESPONSE_FAILED {exception.GetType().Name}");
		}
	}

	Task SendAsync(WebViewWebResourceRequestedEventArgs e, int status, string reason, string mime, byte[] bytes, CancellationToken cancellationToken, long? contentLength = null)
	{
		cancellationToken.ThrowIfCancellationRequested();
		void Respond()
		{
#if !ANDROID
			if (_disposed)
				return;
#endif
			cancellationToken.ThrowIfCancellationRequested();
			var body = e.Method == "HEAD" ? null : new MemoryStream(bytes, writable: false);
			var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				["Content-Type"] = mime,
				["Content-Length"] = (contentLength ?? bytes.Length).ToString(System.Globalization.CultureInfo.InvariantCulture),
				["Cache-Control"] = "no-store"
			};
#if ANDROID
			var type = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(mime);
			e.PlatformArgs!.Response = new global::Android.Webkit.WebResourceResponse(
				type.MediaType, type.CharSet ?? "UTF-8", status, string.IsNullOrEmpty(reason) ? "Response" : reason,
				headers, body);
#else
			e.SetResponse(status, reason, headers, body);
#if IOS || MACCATALYST
			body?.Dispose();
#endif
#endif
			lock (_gate)
			{
				if (_requests.Count == 64)
					_requests.Dequeue();
				_requests.Enqueue(new RequestRecord(e.Uri.PathAndQuery, status, mime));
			}
			Console.WriteLine($"HYBRIDWEBAPP_PROXY {e.Method} {e.Uri.PathAndQuery} {status} {mime}");
		}
#if ANDROID
		Respond();
		return Task.CompletedTask;
#else
		return MainThread.InvokeOnMainThreadAsync(Respond);
#endif
	}

	internal (long Total, int Active, int Canceled, int Failed, RequestRecord[] Requests) GetMetrics()
	{
		lock (_gate)
			return (_total, _active, _canceled, _failed, _requests.ToArray());
	}

	public void Dispose()
	{
		lock (_gate)
		{
			if (_disposed)
				return;
			_disposed = true;
			_shutdown.Cancel();
			_document.Cancel();
		}
		_view.WebResourceRequested -= ResourceRequested;
		_view.WebViewInitialized -= Initialized;
		_client.Dispose();
		_inspector?.Dispose();
		_platform.Dispose();
		_document.Dispose();
		_shutdown.Dispose();
	}

	internal sealed record RequestRecord(string PathAndQuery, int Status, string Mime);

}
