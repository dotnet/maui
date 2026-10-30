using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Gtk.Web;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// What <see cref="GtkWebViewManager"/> needs from a web view: load a URI and run a script. Implemented over WebKitGTK by the handler;
	/// a fake in the tests.
	/// </summary>
	internal interface IGtkBlazorTransport
	{
		/// <summary>Navigates the web view to an absolute URI.</summary>
		void LoadUri(string uri);

		/// <summary>Runs a script in the page, ignoring its result.</summary>
		void RunScript(string script);
	}

	/// <summary>
	/// A <see cref="WebViewManager"/> for WebKitGTK. The page is served through the <c>app://0.0.0.1/</c> custom scheme
	/// (<see cref="RespondAsync"/>); messages travel as <c>window.external.sendMessage</c> (posted to
	/// <c>webkit.messageHandlers.webwindowinterop</c>) and <c>__dispatchMessageCallback("...")</c>.
	/// </summary>
	internal sealed class GtkWebViewManager : WebViewManager
	{
		/// <summary>The origin Blazor is served on. <c>https</c> cannot be served by an app on WebKit, so the <c>app</c> scheme is used (as on iOS).</summary>
		internal static readonly Uri AppOriginUri = new(GtkWebViewScripts.AppScheme + "://" + BlazorWebView.AppHostAddress + "/");

		readonly IGtkBlazorTransport _transport;
		readonly string _contentRootRelativeToAppRoot;
		readonly ILogger? _logger;

		/// <summary>Creates the manager.</summary>
		/// <param name="transport">The web view.</param>
		/// <param name="provider">The application services.</param>
		/// <param name="dispatcher">Marshals calls to the UI thread.</param>
		/// <param name="fileProvider">Serves the static files of the app.</param>
		/// <param name="jsComponents">The JavaScript root component configuration.</param>
		/// <param name="contentRootRelativeToAppRoot">The folder of the host page, relative to the app package.</param>
		/// <param name="hostPageRelativePath">The host page, relative to <paramref name="contentRootRelativeToAppRoot"/>.</param>
		/// <param name="logger">The logger, if any.</param>
		public GtkWebViewManager(
			IGtkBlazorTransport transport,
			IServiceProvider provider,
			Dispatcher dispatcher,
			IFileProvider fileProvider,
			JSComponentConfigurationStore jsComponents,
			string contentRootRelativeToAppRoot,
			string hostPageRelativePath,
			ILogger? logger = null)
			: base(provider, dispatcher, AppOriginUri, fileProvider, jsComponents, hostPageRelativePath)
		{
			_transport = transport ?? throw new ArgumentNullException(nameof(transport));
			_contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;
			_logger = logger;
		}

		/// <inheritdoc/>
		protected override void NavigateCore(Uri absoluteUri) => _transport.LoadUri(absoluteUri.ToString());

		/// <inheritdoc/>
		protected override void SendMessage(string message) => _transport.RunScript(DispatchMessageScript(message));

		/// <summary>The script that hands a .NET-to-JavaScript message to the callbacks <c>window.external.receiveMessage</c> registered.</summary>
		/// <param name="message">The message.</param>
		internal static string DispatchMessageScript(string message) =>
			"__dispatchMessageCallback(\"" + JavaScriptEncoder.Default.Encode(message) + "\")";

		/// <summary>Delivers a message the page posted.</summary>
		/// <param name="source">The URI of the page.</param>
		/// <param name="message">The message.</param>
		internal void MessageReceivedInternal(Uri source, string message) => MessageReceived(source, message);

		/// <summary>Resolves a URL to its content; hot-reloaded content (when enabled) replaces the file's.</summary>
		internal bool TryGetResponseContentInternal(string uri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out IDictionary<string, string> headers)
		{
			var defaultResult = TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);
			var hotReloadedResult = StaticContentHotReloadManager.TryReplaceResponseContent(_contentRootRelativeToAppRoot, uri, ref statusCode, ref content, headers);
			return defaultResult || hotReloadedResult;
		}

		/// <summary>Answers a request to the app origin: the framework files, the host page and the app's static files.</summary>
		/// <param name="request">The request.</param>
		internal async Task<GtkUriSchemeResponse> RespondAsync(GtkUriSchemeRequest request)
		{
			// Pages fall back on the host page (client-side routing); files with an extension never do.
			var allowFallbackOnHostPage = AppOriginUri.IsBaseOfPage(request.Url);
			var url = QueryStringHelper.RemovePossibleQueryString(request.Url);

			_logger?.HandlingWebRequest(url);

			// WebViewManager answers an unknown file with 404 and an explanatory text body; the app gets an empty 404.
			if (!TryGetResponseContentInternal(url, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers) || statusCode == 404)
			{
				content?.Dispose();
				_logger?.ResponseContentNotFound(url);
				return GtkUriSchemeResponse.NotFound();
			}

			using (content)
			{
				using var buffer = new MemoryStream();
				await content.CopyToAsync(buffer);

				headers.TryGetValue("Content-Type", out var contentType);
				_logger?.ResponseContentBeingSent(url, statusCode);

				return new GtkUriSchemeResponse(statusCode, statusMessage, contentType, buffer.ToArray());
			}
		}
	}
}
