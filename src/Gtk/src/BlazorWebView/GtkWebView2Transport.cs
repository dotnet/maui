using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Gtk.Web;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// <see cref="IGtkBlazorTransport"/> over the WebView2 control of a <see cref="MauiGtkWebView2Host"/> (Windows).
	/// </summary>
	/// <remarks>
	/// <para>The app is served from <c>https://0.0.0.1/</c>: <c>WebResourceRequested</c> (filter <c>https://0.0.0.1/*</c>) is answered by
	/// <see cref="GtkWebViewManager.RespondAsync"/>, so nothing listens on the network. <c>window.external.sendMessage</c> /
	/// <c>receiveMessage</c> are a document-created script over <c>chrome.webview</c>; .NET-to-page messages are
	/// <c>PostWebMessageAsString</c> and page-to-.NET messages <c>WebMessageReceived</c>, as in the Windows <c>WebView2WebViewManager</c>.</para>
	/// <para>WebView2 creates its controller once the host is realized and creates a new one after a re-realization. The transport configures every
	/// new <see cref="CoreWebView2"/> as soon as it exists and navigates only after the shim script is registered, so the first document always
	/// has <c>window.external</c>. A navigation requested earlier is kept (the last one wins) and run then. After a re-realization the transport, not the engine (<c>ReplayOnRecreate</c> is off while it is attached), navigates the new control to the app page the closed one showed.</para>
	/// <para>Navigations the page starts (links, <c>window.open</c>) are reported to <c>navigationFilter</c>, which returns <see langword="true"/> to cancel them.</para>
	/// </remarks>
	[SupportedOSPlatform("windows")]
	internal sealed class WebView2Transport : IGtkBlazorTransport, IDisposable
	{
		readonly MauiGtkWebView2Host _host;
		readonly MauiGtkWebView2Engine _engine;
		readonly Func<GtkWebViewManager?> _manager;
		readonly Func<Uri, bool> _navigationFilter;
		readonly bool _developerTools;
		readonly ILogger? _logger;
		CoreWebView2? _core;
		string? _desiredUri;
		bool _ready;
		int _generation;
		bool _disposed;

		/// <summary>Creates the transport and configures the control of <paramref name="host"/> now or when it exists.</summary>
		/// <param name="host">The host of the web view.</param>
		/// <param name="manager">The manager that answers requests and receives messages (it is created after the transport).</param>
		/// <param name="navigationFilter">Called for navigations the page starts; <see langword="true"/> cancels one.</param>
		/// <param name="developerTools">Whether the developer tools (F12, context menu) are enabled.</param>
		/// <param name="logger">The logger, if any.</param>
		public WebView2Transport(MauiGtkWebView2Host host, Func<GtkWebViewManager?> manager, Func<Uri, bool> navigationFilter, bool developerTools, ILogger? logger = null)
		{
			_host = host;
			_engine = host.Engine;
			_manager = manager;
			_navigationFilter = navigationFilter;
			_developerTools = developerTools;
			_logger = logger;

			_host.CoreWebView2Initialized += OnCoreInitialized;
			_host.ControllerClosing += OnControllerClosing;
			_engine.Navigating += OnNavigating;
			// The transport owns the navigation of every new control: it must register its script before the page loads.
			_engine.ReplayOnRecreate = false;

			if (_host.CoreWebView2 is { } core)
				OnCoreInitialized(_host, core);
		}

		/// <summary>Whether the control is configured and navigations are sent to it (otherwise they wait).</summary>
		internal bool IsReady => _ready;

		/// <inheritdoc/>
		public void LoadUri(string uri)
		{
			_desiredUri = uri;
			if (_ready)
				_engine.LoadUri(uri);
		}

		/// <inheritdoc/>
		public void PostMessage(string message)
		{
			// Blazor sends nothing before the page attached itself, which is after the control exists.
			if (_ready && _core is { } core)
				core.PostWebMessageAsString(message);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_host.CoreWebView2Initialized -= OnCoreInitialized;
			_host.ControllerClosing -= OnControllerClosing;
			_engine.Navigating -= OnNavigating;
			_engine.ReplayOnRecreate = true;
			Detach();
		}

		void OnCoreInitialized(object? sender, CoreWebView2 core)
		{
			if (_disposed)
				return;

			Detach();

			_core = core;
			var generation = ++_generation;

			core.Settings.AreDevToolsEnabled = _developerTools;
			core.Settings.AreDefaultContextMenusEnabled = _developerTools;
			core.AddWebResourceRequestedFilter(WebView2BlazorProtocol.RequestFilter, CoreWebView2WebResourceContext.All);
			core.WebResourceRequested += OnWebResourceRequested;
			core.WebMessageReceived += OnWebMessageReceived;

			_ = RegisterScriptAsync(core, generation);
		}

		async Task RegisterScriptAsync(CoreWebView2 core, int generation)
		{
			Exception? failure = null;
			try
			{
				await core.AddScriptToExecuteOnDocumentCreatedAsync(WebView2BlazorProtocol.InitScript);
			}
			catch (Exception e)
			{
				failure = e;
			}

			GtkRuntime.Post(() =>
			{
				if (_disposed || generation != _generation)
					return;

				if (failure is not null)
					_logger?.LogError(failure, "Failed to register the BlazorWebView script: {Message}", failure.Message);

				_ready = true;
				if (_desiredUri is { } uri)
					_engine.LoadUri(uri);
			});
		}

		void OnControllerClosing(object? sender, EventArgs e)
		{
			// The next control continues where this one is (the route the user reached), not at the start page.
			if (_core is { } core && Uri.TryCreate(core.Source, UriKind.Absolute, out var source) && source.Scheme != "about"
				&& GtkWebViewManager.WebView2AppOriginUri.IsBaseOf(source))
				_desiredUri = source.AbsoluteUri;

			Detach();
		}

		void Detach()
		{
			_generation++;
			_ready = false;

			if (_core is { } core)
			{
				core.WebResourceRequested -= OnWebResourceRequested;
				core.WebMessageReceived -= OnWebMessageReceived;
			}

			_core = null;
		}

		void OnNavigating(object? sender, GtkWebNavigatingEventArgs args)
		{
			if (Uri.TryCreate(args.Uri, UriKind.Absolute, out var uri) && uri.Scheme != "about" && _navigationFilter(uri))
				args.Cancel = true;
		}

		void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
		{
			try
			{
				if (_manager() is not { } manager || !Uri.TryCreate(args.Source, UriKind.Absolute, out var source))
					return;

				string message;
				try
				{
					message = args.TryGetWebMessageAsString();
				}
				catch (ArgumentException)
				{
					// Not a string message: not ours.
					return;
				}

				// WebViewManager ignores messages from pages outside the app origin.
				manager.MessageReceivedInternal(source, message);
			}
			catch (Exception e)
			{
				// A failing component must not unwind into the WebView2 event dispatch.
				_logger?.LogError(e, "Failed to process a BlazorWebView message: {Message}", e.Message);
			}
		}

		void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs args)
		{
			if (_manager() is not { } manager || sender is not CoreWebView2 core)
				return;

			var deferral = args.GetDeferral();
			var environment = core.Environment;
			var request = args.Request;
			byte[] body = Array.Empty<byte>();

			try
			{
				if (request.Content is { } content)
				{
					using var buffer = new MemoryStream();
					content.CopyTo(buffer);
					body = buffer.ToArray();
				}
			}
			catch (Exception e) when (e is IOException or System.Runtime.InteropServices.COMException)
			{
				body = Array.Empty<byte>();
			}

			var task = RespondAsync(manager, new GtkUriSchemeRequest(request.Uri, request.Method ?? "GET", body, core.Source));
			_ = task.ContinueWith(
				t => GtkRuntime.Post(() =>
				{
					MemoryStream? stream = null;
					try
					{
						var response = t.IsCompletedSuccessfully ? t.Result : GtkUriSchemeResponse.NotFound();
						stream = new MemoryStream(response.Body, writable: false);
						args.Response = environment.CreateWebResourceResponse(
							stream,
							response.Status,
							response.Reason,
							WebView2BlazorProtocol.BuildResponseHeaders(response));
						stream = null; // owned by the response now
					}
					catch (Exception e)
					{
						_logger?.LogError(e, "Failed to answer a BlazorWebView request: {Message}", e.Message);
					}
					finally
					{
						stream?.Dispose();

						try
						{
							deferral.Complete();
						}
						catch (Exception e) when (e is System.Runtime.InteropServices.COMException or ObjectDisposedException or InvalidOperationException)
						{
							// The controller was closed meanwhile: nobody waits for the answer, and this runs on the GTK main loop.
							_logger?.LogDebug(e, "The BlazorWebView request was abandoned: {Message}", e.Message);
						}
					}
				}),
				TaskScheduler.Default);
		}

		async Task<GtkUriSchemeResponse> RespondAsync(GtkWebViewManager manager, GtkUriSchemeRequest request)
		{
			try
			{
				return await manager.RespondAsync(request);
			}
			catch (Exception e)
			{
				_logger?.LogError(e, "Failed to serve {Url}: {Message}", request.Url, e.Message);
				return new GtkUriSchemeResponse(500, "Internal Server Error", null, null);
			}
		}
	}

	/// <summary>The WebView2 protocol of the Blazor app: what the page and the host agree on, independent of the WebView2 types.</summary>
	internal static class WebView2BlazorProtocol
	{
		/// <summary>The filter of the requests answered by the manager.</summary>
		internal static readonly string RequestFilter = GtkWebViewManager.WebView2AppOriginUri.AbsoluteUri + "*";

		/// <summary>
		/// The script registered on every document: <c>window.external</c> over <c>chrome.webview</c>, and the start of Blazor once the page
		/// loaded (the host page loads <c>blazor.webview.js</c> with <c>autostart="false"</c>).
		/// </summary>
		internal const string InitScript = @"
			window.external = {
				sendMessage: function(message) {
					window.chrome.webview.postMessage(message);
				},
				receiveMessage: function(callback) {
					window.chrome.webview.addEventListener('message', function(e) { callback(e.data); });
				}
			};

			document.addEventListener('DOMContentLoaded', function() {
				if (window.Blazor) {
					window.Blazor.start();
				}
			});

			window.addEventListener('pageshow', function(event) {
				if (event.persisted) {
					window.location.reload();
				}
			});
		";

		/// <summary>The headers of the response for <paramref name="response"/>, one per line.</summary>
		internal static string BuildResponseHeaders(GtkUriSchemeResponse response)
		{
			var headers = new StringBuilder();
			if (!string.IsNullOrEmpty(response.ContentType))
				headers.Append("Content-Type: ").Append(response.ContentType).Append("\r\n");

			// The app content is read from the package on every request and must never be cached stale (hot reload, updates).
			headers.Append("Cache-Control: no-cache, max-age=0, must-revalidate, no-store\r\n");
			headers.Append("Access-Control-Allow-Origin: *");
			return headers.ToString();
		}
	}
}
