using System;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Describes a request for static content served by a <see cref="BlazorWebView"/>. An instance is passed to the
	/// callback set on <see cref="BlazorWebView.StaticContentCacheControlProvider"/> so the application can decide
	/// which <c>Cache-Control</c> header value to send for the resource.
	/// </summary>
	public sealed class BlazorWebViewStaticContentRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlazorWebViewStaticContentRequest"/> class.
		/// </summary>
		/// <param name="uri">The absolute URI of the requested static content.</param>
		/// <param name="contentType">The resolved MIME content type of the requested static content.</param>
		public BlazorWebViewStaticContentRequest(Uri uri, string contentType)
		{
			ArgumentNullException.ThrowIfNull(uri);
			ArgumentNullException.ThrowIfNull(contentType);

			Uri = uri;
			ContentType = contentType;
		}

		/// <summary>
		/// Gets the absolute URI of the requested static content.
		/// </summary>
		public Uri Uri { get; }

		/// <summary>
		/// Gets the resolved MIME content type of the requested static content, for example <c>image/png</c>.
		/// </summary>
		public string ContentType { get; }
	}
}
