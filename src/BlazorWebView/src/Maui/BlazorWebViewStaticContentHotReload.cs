using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
	/// Call <see cref="TryAttachToWebViewManager(WebViewManager)"/> once, right after creating its
	/// <see cref="WebViewManager"/> and before navigating, so the notifier root component is registered.
	/// </description></item>
	/// <item><description>
	/// Call <see cref="TryGetUpdatedStaticContent(string, string, out Stream, out string)"/> while resolving a
	/// static content request, so hot-reloaded content replaces the on-disk content.
	/// </description></item>
	/// <item><description>
	/// Call <see cref="TryDetachFromWebViewManager(WebViewManager)"/> from its disconnect or disposal path,
	/// before disposing the <see cref="WebViewManager"/>, so a handler that is reconnected can attach again.
	/// </description></item>
	/// </list>
	/// Both members are inert when hot reload is unavailable (that is, when
	/// <see cref="System.Reflection.Metadata.MetadataUpdater.IsSupported"/> is <see langword="false"/>), so
	/// handlers can call them unconditionally. Static content hot reload is a development-time feature and is
	/// independent of Razor component hot reload, which does not require any handler participation.
	/// </remarks>
	public static class BlazorWebViewStaticContentHotReload
	{
		/// <summary>
		/// Registers the static content hot reload notifier with the specified <see cref="WebViewManager"/>
		/// when hot reload is supported by the current runtime.
		/// </summary>
		/// <param name="webViewManager">The <see cref="WebViewManager"/> to attach to.</param>
		/// <returns>
		/// A <see cref="Task"/> that completes when the notifier has been registered, or <see langword="null"/>
		/// when hot reload is not supported by the current runtime and nothing was attached. Awaiting the task
		/// is optional; handlers that attach before navigating can ignore it, because registration completes
		/// synchronously until a page is attached.
		/// </returns>
		/// <remarks>
		/// Attaching is idempotent per <see cref="WebViewManager"/> instance: repeat calls for the same manager
		/// return the task produced by the first successful attach rather than registering the notifier again.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webViewManager"/> is <see langword="null"/>.</exception>
		public static Task? TryAttachToWebViewManager(WebViewManager webViewManager)
		{
			ArgumentNullException.ThrowIfNull(webViewManager);

			return StaticContentHotReloadManager.TryAttachToWebViewManager(webViewManager);
		}

		/// <summary>
		/// Removes the static content hot reload notifier from the specified <see cref="WebViewManager"/>,
		/// when it was previously attached by <see cref="TryAttachToWebViewManager(WebViewManager)"/>.
		/// </summary>
		/// <param name="webViewManager">The <see cref="WebViewManager"/> to detach from.</param>
		/// <returns>
		/// A <see cref="Task"/> that completes when the notifier has been removed, or <see langword="null"/>
		/// when nothing was attached to this manager and there was nothing to remove.
		/// </returns>
		/// <remarks>
		/// Handlers should call this from their disconnect or disposal path, before disposing the
		/// <see cref="WebViewManager"/>, so a handler that is later reconnected can attach again. Detaching is
		/// idempotent: calling it when nothing is attached returns <see langword="null"/> rather than throwing.
		/// Detaching is not required purely to avoid a leak — the attachment is tracked weakly and the notifier
		/// unsubscribes when the root component is disposed — but it is required for a subsequent
		/// <see cref="TryAttachToWebViewManager(WebViewManager)"/> on the same manager to take effect.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webViewManager"/> is <see langword="null"/>.</exception>
		public static Task? TryDetachFromWebViewManager(WebViewManager webViewManager)
		{
			ArgumentNullException.ThrowIfNull(webViewManager);

			return StaticContentHotReloadManager.TryDetachFromWebViewManager(webViewManager);
		}

		/// <summary>
		/// Gets hot-reloaded content for a static content request, when an update for that content is available.
		/// </summary>
		/// <param name="contentRootRelativePath">The content root of the app's static assets relative to the
		/// app root, as passed to the <see cref="WebViewManager"/>.</param>
		/// <param name="requestAbsoluteUri">The absolute URI of the request being served.</param>
		/// <param name="content">When this method returns <see langword="true"/>, a new readable
		/// <see cref="Stream"/> over the hot-reloaded content; otherwise <see langword="null"/>. The caller owns
		/// this stream and is responsible for disposing it.</param>
		/// <param name="contentType">When this method returns <see langword="true"/>, the content type the
		/// hot reload payload specifies, or <see langword="null"/> when it does not specify one and the caller
		/// should keep the content type it already resolved; otherwise <see langword="null"/>.</param>
		/// <returns><see langword="true"/> if hot-reloaded content is available for the request; otherwise <see langword="false"/>.</returns>
		/// <remarks>
		/// This method only reports content. The caller decides what to do with the response it was already
		/// building — typically replacing the body, setting the status code to <c>200</c>, applying
		/// <paramref name="contentType"/> when it is not <see langword="null"/>, and disposing whatever content
		/// it had resolved beforehand.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="contentRootRelativePath"/> or
		/// <paramref name="requestAbsoluteUri"/> is <see langword="null"/>.</exception>
		public static bool TryGetUpdatedStaticContent(
			string contentRootRelativePath,
			string requestAbsoluteUri,
			out Stream? content,
			out string? contentType)
		{
			ArgumentNullException.ThrowIfNull(contentRootRelativePath);
			ArgumentNullException.ThrowIfNull(requestAbsoluteUri);

			if (StaticContentHotReloadManager.TryGetUpdatedContent(
					contentRootRelativePath,
					requestAbsoluteUri,
					out var bytes,
					out contentType))
			{
				content = new MemoryStream(bytes!, writable: false);
				return true;
			}

			content = null;
			return false;
		}
	}
}
