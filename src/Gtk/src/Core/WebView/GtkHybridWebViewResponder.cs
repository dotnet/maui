using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// Answers the requests a HybridWebView page makes to its app origin (<c>app://0.0.0.1/</c>): the bundled <c>hybridwebview.js</c>, the
	/// <c>__hwvInvokeDotNet</c> endpoint and the static files of the hybrid root. The WebKit-free counterpart of the iOS scheme handler and the
	/// Windows resource handler, so it runs (and is tested) without a web view.
	/// </summary>
	/// <remarks>
	/// WebKitGTK does not expose the headers of a scheme request, so the <c>X-Maui-Invoke-Token</c>/<c>Origin</c> check the other platforms apply
	/// to <c>__hwvInvokeDotNet</c> is replaced by two checks: the request must be a <c>POST</c> with a body, and it must come from a page
	/// of the app origin (<see cref="GtkUriSchemeRequest.PageUri"/>). WebView2 exposes the headers: a request that carries
	/// <see cref="GtkUriSchemeRequest.Headers"/> (served on <see cref="WebView2OriginUri"/>) gets the header check of Windows, Android and iOS instead.
	/// </remarks>
	internal sealed class GtkHybridWebViewResponder
	{
		/// <summary>The app origin HybridWebView serves on GTK. <c>https</c> cannot be served by an app on WebKit, so the <c>app</c> scheme is used.</summary>
		internal static readonly Uri AppOriginUri = new(GtkWebViewScripts.AppScheme + "://0.0.0.1/");

		/// <summary>The app origin HybridWebView serves on WebView2: <c>https://0.0.0.1/</c>, as on Windows and Android.</summary>
		internal static readonly Uri WebView2OriginUri = new("https://0.0.0.1/");

		readonly Uri _origin;
		readonly Func<string?> _hybridRoot;
		readonly Func<string?> _defaultFile;
		readonly Func<byte[], Task<byte[]?>> _invokeDotNet;
		readonly Func<string, Task<Stream?>> _openAsset;
		readonly Func<string, Stream?> _openEmbedded;
		readonly ILogger? _logger;

		/// <summary>Creates the responder.</summary>
		/// <param name="hybridRoot">The folder (inside the app package) the web app lives in.</param>
		/// <param name="defaultFile">The file served for the origin root.</param>
		/// <param name="invokeDotNet">Runs a JavaScript-to-.NET invocation (body in, JSON out).</param>
		/// <param name="openAsset">Opens an app package file, or returns <see langword="null"/> when there is none.</param>
		/// <param name="openEmbedded">Opens an embedded resource, or returns <see langword="null"/>.</param>
		/// <param name="logger">The logger, if any.</param>
		/// <param name="origin">The app origin to serve, or <see langword="null"/> for <see cref="AppOriginUri"/>.</param>
		public GtkHybridWebViewResponder(
			Func<string?> hybridRoot,
			Func<string?> defaultFile,
			Func<byte[], Task<byte[]?>> invokeDotNet,
			Func<string, Task<Stream?>> openAsset,
			Func<string, Stream?> openEmbedded,
			ILogger? logger = null,
			Uri? origin = null)
		{
			_origin = origin ?? AppOriginUri;
			_hybridRoot = hybridRoot;
			_defaultFile = defaultFile;
			_invokeDotNet = invokeDotNet;
			_openAsset = openAsset;
			_openEmbedded = openEmbedded;
			_logger = logger;
		}

		/// <summary>Answers one request.</summary>
		/// <param name="request">The request.</param>
		internal async Task<GtkUriSchemeResponse> RespondAsync(GtkUriSchemeRequest request)
		{
			var url = WebUtils.RemovePossibleQueryString(request.Url);

			if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !_origin.IsBaseOf(uri))
			{
				_logger?.LogDebug("Request for {Url} is not for the app origin.", url);
				return GtkUriSchemeResponse.NotFound();
			}

			var relativePath = WebUtils.ResolveRelativePath(_origin, uri);
			if (relativePath is null)
			{
				_logger?.LogDebug("Request for {Url} resolved to an invalid path.", url);
				return GtkUriSchemeResponse.NotFound();
			}

			if (relativePath == HybridWebViewHandler.HybridWebViewDotJsPath)
			{
				using var script = _openEmbedded(HybridWebViewHandler.HybridWebViewDotJsPath);
				if (script is not null)
					return GtkUriSchemeResponse.Ok("application/javascript", await ReadAllAsync(script));
			}

			if (relativePath == HybridWebViewHandler.InvokeDotNetPath)
				return await InvokeDotNetAsync(request);

			string? contentType;
			if (string.IsNullOrEmpty(relativePath))
			{
				relativePath = _defaultFile();
				contentType = "text/html";
			}
			else if (!HybridWebViewHandler.ContentTypeProvider.TryGetContentType(relativePath, out contentType))
			{
				contentType = "text/plain";
				_logger?.LogWarning("Could not determine content type for '{RelativePath}'", relativePath);
			}

			var root = _hybridRoot();
			var assetPath = root is null || relativePath is null ? null : FileSystemUtils.Combine(root, relativePath);
			if (assetPath is null)
				return GtkUriSchemeResponse.NotFound();

			using var asset = await _openAsset(assetPath);
			if (asset is null)
			{
				_logger?.LogDebug("Request for {Url} could not be fulfilled.", url);
				return GtkUriSchemeResponse.NotFound();
			}

			_logger?.LogDebug("Request for {Url} will return an app package file.", url);
			return GtkUriSchemeResponse.Ok(contentType, await ReadAllAsync(asset));
		}

		async Task<GtkUriSchemeResponse> InvokeDotNetAsync(GtkUriSchemeRequest request)
		{
			if (request.Headers is { } headers)
			{
				if (!HybridWebViewHandler.HasExpectedHeaders(headers))
				{
					_logger?.LogError("InvokeDotNet endpoint missing or invalid request header");
					return GtkUriSchemeResponse.BadRequest();
				}
			}
			else if (!IsFromOrigin(_origin, request.PageUri))
			{
				_logger?.LogError("InvokeDotNet endpoint called from outside the app origin");
				return GtkUriSchemeResponse.BadRequest();
			}

			if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
			{
				_logger?.LogError("InvokeDotNet endpoint only accepts POST requests. Received: {Method}", request.Method);
				return GtkUriSchemeResponse.MethodNotAllowed();
			}

			if (request.Body.Length == 0)
			{
				_logger?.LogError("InvokeDotNet request body is empty");
				return GtkUriSchemeResponse.BadRequest();
			}

			var content = await _invokeDotNet(request.Body);
			return content is null
				? GtkUriSchemeResponse.NotFound()
				: GtkUriSchemeResponse.Ok("application/json", content);
		}

		/// <summary>Whether a page URI belongs to the app origin; a missing URI does not.</summary>
		/// <param name="pageUri">The URI of the page that made a request or posted a message.</param>
		internal static bool IsFromAppOrigin(string? pageUri) => IsFromOrigin(AppOriginUri, pageUri);

		/// <summary>Whether a page URI belongs to <paramref name="origin"/>; a missing URI does not.</summary>
		/// <param name="origin">The app origin.</param>
		/// <param name="pageUri">The URI of the page that made a request or posted a message.</param>
		internal static bool IsFromOrigin(Uri origin, string? pageUri) =>
			Uri.TryCreate(pageUri, UriKind.Absolute, out var page) && origin.IsBaseOf(page);

		static async Task<byte[]> ReadAllAsync(Stream stream)
		{
			if (stream is MemoryStream memory)
				return memory.ToArray();

			using var buffer = new MemoryStream();
			await stream.CopyToAsync(buffer);
			return buffer.ToArray();
		}
	}
}
