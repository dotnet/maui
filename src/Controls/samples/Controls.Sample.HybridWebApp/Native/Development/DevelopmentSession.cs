using System.Net;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using WebKit;

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
	readonly LoopbackInspector _inspector;
	readonly NavigationObserver _navigation;
	CancellationTokenSource _document = new();
	long _total;
	int _active;
	int _canceled;
	int _failed;
	volatile bool _disposed;

	internal WKWebView? NativeView { get; private set; }
	internal bool IsDisposed => _disposed;
	internal string Mode => _settings.Upstream is null ? "packaged" : "vite";

	public DevelopmentSession(HybridWebView view, DevelopmentSettings settings)
	{
		_view = view;
		_settings = settings;
		_navigation = new NavigationObserver(this);
		view.WebResourceRequested += ResourceRequested;
		view.WebViewInitialized += Initialized;
		_inspector = new LoopbackInspector(this, view, settings);
	}

	void Initialized(object? sender, WebViewInitializedEventArgs e)
	{
		NativeView = e.PlatformArgs?.Sender;
		if (_settings.Upstream is not null && NativeView is { } native)
		{
			if (native.NavigationDelegate is not null)
				throw new InvalidOperationException("The Debug sample must not replace an existing navigation delegate.");
			native.NavigationDelegate = _navigation;
		}
	}

	void StartingDocument()
	{
		if (_disposed)
			return;
		_document.Cancel();
		_document.Dispose();
		_document = new CancellationTokenSource();
	}

	void ResourceRequested(object? sender, WebViewWebResourceRequestedEventArgs e)
	{
		if (_disposed || _settings.Upstream is null || e.Uri.Scheme != "app" || e.Uri.Host != "0.0.0.1" ||
			!e.Uri.IsDefaultPort || e.Uri.UserInfo.Length != 0)
			return;

		var path = Uri.UnescapeDataString(e.Uri.AbsolutePath);
		if (path is "/_framework/hybridwebview.js" or "/__hwvInvokeDotNet" or "/__hwvSendMessage")
		{
			Console.WriteLine($"HYBRIDWEBAPP_NATIVE_ROUTE {e.Uri.PathAndQuery}");
			return;
		}

		e.Handled = true;
		var documentToken = _document.Token;
		var cancellation = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token, documentToken);
		cancellation.CancelAfter(TimeSpan.FromSeconds(10));
		lock (_gate)
		{
			_total++;
			_active++;
		}
		_ = RespondAsync(e, cancellation, documentToken);
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
			if (!_disposed && !documentToken.IsCancellationRequested)
				await SendErrorAsync(e, 504, "Gateway Timeout", "Vite asset fetch timed out.").ConfigureAwait(false);
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

	async Task SendAsync(WebViewWebResourceRequestedEventArgs e, int status, string reason, string mime, byte[] bytes, CancellationToken cancellationToken, long? contentLength = null)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await MainThread.InvokeOnMainThreadAsync(() =>
		{
			if (_disposed)
				return;
			cancellationToken.ThrowIfCancellationRequested();
			using var body = e.Method == "HEAD" ? null : new MemoryStream(bytes, writable: false);
			var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				["Content-Type"] = mime,
				["Content-Length"] = (contentLength ?? bytes.Length).ToString(System.Globalization.CultureInfo.InvariantCulture),
				["Cache-Control"] = "no-store"
			};
			e.SetResponse(status, reason, headers, body);
			lock (_gate)
			{
				if (_requests.Count == 64)
					_requests.Dequeue();
				_requests.Enqueue(new RequestRecord(e.Uri.PathAndQuery, status, mime));
			}
			Console.WriteLine($"HYBRIDWEBAPP_PROXY {e.Method} {e.Uri.PathAndQuery} {status} {mime}");
		}).ConfigureAwait(false);
	}

	internal (long Total, int Active, int Canceled, int Failed, RequestRecord[] Requests) GetMetrics()
	{
		lock (_gate)
			return (_total, _active, _canceled, _failed, _requests.ToArray());
	}

	public void Dispose()
	{
		if (_disposed)
			return;
		_disposed = true;
		_view.WebResourceRequested -= ResourceRequested;
		_view.WebViewInitialized -= Initialized;
		_shutdown.Cancel();
		_document.Cancel();
		_client.Dispose();
		_inspector.Dispose();
		if (NativeView?.NavigationDelegate == _navigation)
			NativeView.WeakNavigationDelegate = null;
		_navigation.Dispose();
		NativeView = null;
		_document.Dispose();
		_shutdown.Dispose();
	}

	internal sealed record RequestRecord(string PathAndQuery, int Status, string Mime);

	sealed class NavigationObserver(DevelopmentSession session) : WKNavigationDelegate
	{
		public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
		{
			// Observe only main-frame navigation, not Vite's index.html?html-proxy source modules.
			if (navigationAction.TargetFrame?.MainFrame == true)
				session.StartingDocument();
			decisionHandler(WKNavigationActionPolicy.Allow);
		}
	}
}
