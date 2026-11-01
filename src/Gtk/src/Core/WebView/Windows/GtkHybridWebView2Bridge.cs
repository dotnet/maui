using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// The WebView2 side of the GTK <c>HybridWebView</c>: serves the app origin (<c>https://0.0.0.1/</c>) through <c>WebResourceRequested</c> with the
	/// same <see cref="GtkHybridWebViewResponder"/> as the WebKit <c>app://</c> scheme, receives <c>chrome.webview.postMessage</c> as
	/// <c>WebMessageReceived</c> and posts raw messages with <c>PostWebMessageAsString</c>; mirrors <c>HybridWebViewHandler.Windows.cs</c>.
	/// </summary>
	/// <remarks>
	/// <para>The bridge configures every <see cref="CoreWebView2"/> the host creates (<see cref="MauiGtkWebView2Host.CoreWebView2Initialized"/>, before
	/// the first navigation, also after a re-realization) and lets go of it when the controller closes. Raw messages sent while there is no
	/// <see cref="CoreWebView2"/> are kept and posted when the next one is configured.</para>
	/// <para>A resource request holds a WebView2 deferral until the responder finished; the answer is set and the deferral completed on the GTK
	/// thread (COM objects of WebView2 are apartment-bound). The response body is a <see cref="MemoryStream"/> that WebView2 reads after the
	/// handler returned, so it is never disposed here.</para>
	/// </remarks>
	internal sealed class GtkHybridWebView2Bridge : IDisposable
	{
		/// <summary>The URL filter of the intercepted requests.</summary>
		internal const string ResourceFilter = "https://0.0.0.1/*";

		readonly MauiGtkWebView2Host _host;
		readonly GtkHybridWebViewResponder _responder;
		readonly Action<string> _messageReceived;
		readonly Func<bool> _developerTools;
		readonly ILogger? _logger;
		readonly List<string> _pending = new();
		CoreWebView2? _core;
		bool _disposed;

		GtkHybridWebView2Bridge(
			MauiGtkWebView2Host host,
			GtkHybridWebViewResponder responder,
			Action<string> messageReceived,
			Func<bool> developerTools,
			ILogger? logger)
		{
			_host = host;
			_responder = responder;
			_messageReceived = messageReceived;
			_developerTools = developerTools;
			_logger = logger;

			host.CoreWebView2Initialized += OnCoreInitialized;
			host.ControllerClosing += OnControllerClosing;

			if (host.CoreWebView2 is { } core)
				Attach(core);
		}

		/// <summary>
		/// Attaches a bridge to <paramref name="platformView"/> when it is a WebView2 host (call on Windows only), or returns <see langword="null"/>.
		/// </summary>
		/// <param name="platformView">The platform view made by <see cref="GtkWebBackend.CreatePlatformView"/>.</param>
		/// <param name="responder">The responder for the <see cref="GtkHybridWebViewResponder.WebView2OriginUri"/> origin.</param>
		/// <param name="messageReceived">Receives each decoded message of a page of the app origin.</param>
		/// <param name="developerTools">Whether the developer tools are enabled.</param>
		/// <param name="logger">The logger, if any.</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static GtkHybridWebView2Bridge? TryCreate(
			global::Gtk.Widget platformView,
			GtkHybridWebViewResponder responder,
			Action<string> messageReceived,
			Func<bool> developerTools,
			ILogger? logger) =>
			platformView is MauiGtkWebView2Host host
				? new GtkHybridWebView2Bridge(host, responder, messageReceived, developerTools, logger)
				: null;

		/// <summary>The engine of the host.</summary>
		internal IGtkWebEngine Engine => _host.Engine;

		/// <summary>
		/// Posts a raw message to the page: URL-encoded, so embedded NUL characters survive WebView2's null-terminated string marshalling
		/// (<c>hybridwebview.js</c> decodes it).
		/// </summary>
		/// <param name="message">The message.</param>
		internal void SendRawMessage(string message)
		{
			if (_disposed)
				return;

			if (_core is { } core)
				core.PostWebMessageAsString(EncodeMessage(message));
			else
				_pending.Add(message);
		}

		/// <summary>The encoding of a message posted to the page.</summary>
		internal static string EncodeMessage(string message) => Uri.EscapeDataString(message);

		/// <summary>The inverse of the encoding the page applies to a message it posts (<c>encodeURIComponent</c>).</summary>
		internal static string DecodeMessage(string message) => Uri.UnescapeDataString(message);

		/// <summary>The response headers WebView2 gets for <paramref name="response"/>: <c>Content-Length</c> and <c>Content-Type</c>, when known.</summary>
		internal static string BuildHeaders(GtkUriSchemeResponse response)
		{
			var headers = string.Empty;
			if (response.Body.Length > 0)
				headers += "Content-Length: " + response.Body.Length + "\r\n";
			if (response.ContentType is not null)
				headers += "Content-Type: " + response.ContentType + "\r\n";

			return headers;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_host.CoreWebView2Initialized -= OnCoreInitialized;
			_host.ControllerClosing -= OnControllerClosing;
			Detach();
			_pending.Clear();
		}

		void OnCoreInitialized(object? sender, CoreWebView2 core) => Attach(core);

		void OnControllerClosing(object? sender, EventArgs e) => Detach();

		void Attach(CoreWebView2 core)
		{
			if (_disposed || ReferenceEquals(_core, core))
				return;

			Detach();
			_core = core;

			core.Settings.IsWebMessageEnabled = true;
			core.Settings.AreDevToolsEnabled = _developerTools();
			core.AddWebResourceRequestedFilter(ResourceFilter, CoreWebView2WebResourceContext.All);
			core.WebResourceRequested += OnWebResourceRequested;
			core.WebMessageReceived += OnWebMessageReceived;

			if (_pending.Count > 0)
			{
				var pending = _pending.ToArray();
				_pending.Clear();
				foreach (var message in pending)
					core.PostWebMessageAsString(EncodeMessage(message));
			}
		}

		void Detach()
		{
			if (_core is { } core)
			{
				core.WebResourceRequested -= OnWebResourceRequested;
				core.WebMessageReceived -= OnWebMessageReceived;
			}

			_core = null;
		}

		void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
		{
			try
			{
				if (!GtkHybridWebViewResponder.IsFromOrigin(GtkHybridWebViewResponder.WebView2OriginUri, e.Source))
				{
					_logger?.LogDebug("Ignoring web message from an unrecognized source.");
					return;
				}

				_messageReceived(DecodeMessage(e.TryGetWebMessageAsString()));
			}
			catch (Exception ex)
			{
				// A malformed or non-string message must not unwind into the WebView2 event.
				_logger?.LogError(ex, "Failed to process a HybridWebView message: {Message}", ex.Message);
			}
		}

		void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
		{
			if (sender is not CoreWebView2 core)
				return;

			var url = e.Request.Uri;
			_logger?.LogDebug("Intercepting request for {Url}.", url);

			GtkUriSchemeRequest request;
			try
			{
				request = ReadRequest(e.Request);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to read the request for {Url}.", url);
				return;
			}

			// The answer is asynchronous: WebView2 waits for the deferral.
			Respond(core, e, e.GetDeferral(), request);
		}

		static GtkUriSchemeRequest ReadRequest(CoreWebView2WebResourceRequest request)
		{
			var headers = new List<KeyValuePair<string, string>>();
			foreach (var header in request.Headers)
				headers.Add(header);

			var body = Array.Empty<byte>();
			if (request.Content is { } content)
			{
				using var buffer = new MemoryStream();
				content.CopyTo(buffer);
				body = buffer.ToArray();
			}

			return new GtkUriSchemeRequest(request.Uri, string.IsNullOrEmpty(request.Method) ? "GET" : request.Method, body, null, headers);
		}

		async void Respond(CoreWebView2 core, CoreWebView2WebResourceRequestedEventArgs e, CoreWebView2Deferral deferral, GtkUriSchemeRequest request)
		{
			GtkUriSchemeResponse response;
			try
			{
				response = await _responder.RespondAsync(request).ConfigureAwait(true);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to answer the request for {Url}.", request.Url);
				response = GtkUriSchemeResponse.ServerError();
			}

			// WebView2 objects belong to the GTK thread; the responder may have completed elsewhere.
			GtkRuntime.Post(() =>
			{
				try
				{
					// A closed controller has nobody to answer to.
					if (!_disposed && ReferenceEquals(_core, core))
					{
						e.Response = core.Environment.CreateWebResourceResponse(
							response.Body.Length > 0 ? new MemoryStream(response.Body, writable: false) : null,
							response.Status,
							response.Reason,
							BuildHeaders(response));
					}
				}
				catch (Exception ex)
				{
					_logger?.LogError(ex, "Failed to answer the request for {Url}.", request.Url);
				}
				finally
				{
					try
					{
						deferral.Complete();
					}
					catch (Exception)
					{
						// The web view went away with the request pending.
					}
				}
			});
		}
	}
}
