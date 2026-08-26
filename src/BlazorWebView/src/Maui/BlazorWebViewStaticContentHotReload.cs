using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Provides the seam that lets a handler implementing <see cref="IBlazorWebViewHandler"/> participate in
	/// MAUI Blazor static content hot reload, which serves updated <c>wwwroot</c> assets (most notably CSS)
	/// without restarting the app.
	/// </summary>
	/// <remarks>
	/// A handler participates in two places, mirroring what the built-in handlers do:
	/// <list type="number">
	/// <item><description>
	/// Call <see cref="AttachToWebViewManagerIfEnabled(WebViewManager)"/> once, right after creating its
	/// <see cref="WebViewManager"/> and before navigating, so the notifier root component is registered.
	/// </description></item>
	/// <item><description>
	/// Call <see cref="TryReplaceResponseContent(string, string, ref int, ref Stream, IDictionary{string, string})"/>
	/// while resolving a static content request, so hot-reloaded content replaces the on-disk content.
	/// </description></item>
	/// </list>
	/// Both members are no-ops when hot reload is unavailable (that is, when
	/// <see cref="System.Reflection.Metadata.MetadataUpdater.IsSupported"/> is <see langword="false"/>), so
	/// handlers can call them unconditionally. Static content hot reload is a development-time feature and is
	/// independent of Razor component hot reload, which does not require any handler participation.
	/// </remarks>
	public static class BlazorWebViewStaticContentHotReload
	{
		/// <summary>
		/// Registers the static content hot reload notifier with the specified <see cref="WebViewManager"/>
		/// when hot reload is supported by the current runtime; otherwise does nothing.
		/// </summary>
		/// <param name="webViewManager">The <see cref="WebViewManager"/> to attach to.</param>
		/// <remarks>
		/// Call this once per <see cref="WebViewManager"/> instance, after construction and before navigating.
		/// Calling it more than once for the same manager throws, because the notifier uses a fixed root
		/// component selector.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webViewManager"/> is <see langword="null"/>.</exception>
		public static void AttachToWebViewManagerIfEnabled(WebViewManager webViewManager)
		{
			ArgumentNullException.ThrowIfNull(webViewManager);

			StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(webViewManager);
		}

		/// <summary>
		/// Replaces the response for a static content request with hot-reloaded content when an update for
		/// that content is available; otherwise leaves the response untouched.
		/// </summary>
		/// <param name="contentRootRelativePath">The content root of the app's static assets relative to the
		/// app root, as passed to the <see cref="WebViewManager"/>.</param>
		/// <param name="requestAbsoluteUri">The absolute URI of the request being served.</param>
		/// <param name="responseStatusCode">The response status code. Set to <c>200</c> when content is replaced.</param>
		/// <param name="responseContent">The response content. Replaced with the hot-reloaded content, and the
		/// original stream is closed, when content is replaced.</param>
		/// <param name="responseHeaders">The response headers. The <c>Content-Type</c> header is updated when the
		/// hot reload payload specifies one.</param>
		/// <returns><see langword="true"/> if the response was replaced with hot-reloaded content; otherwise <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="contentRootRelativePath"/>,
		/// <paramref name="requestAbsoluteUri"/>, <paramref name="responseContent"/> or
		/// <paramref name="responseHeaders"/> is <see langword="null"/>.</exception>
		public static bool TryReplaceResponseContent(
			string contentRootRelativePath,
			string requestAbsoluteUri,
			ref int responseStatusCode,
			ref Stream responseContent,
			IDictionary<string, string> responseHeaders)
		{
			ArgumentNullException.ThrowIfNull(contentRootRelativePath);
			ArgumentNullException.ThrowIfNull(requestAbsoluteUri);
			ArgumentNullException.ThrowIfNull(responseContent);
			ArgumentNullException.ThrowIfNull(responseHeaders);

			return StaticContentHotReloadManager.TryReplaceResponseContent(
				contentRootRelativePath,
				requestAbsoluteUri,
				ref responseStatusCode,
				ref responseContent,
				responseHeaders);
		}
	}
}
