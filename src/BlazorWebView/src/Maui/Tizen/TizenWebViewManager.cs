using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using NWebView = Tizen.NUI.BaseComponents.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// An implementation of <see cref="WebViewManager"/> that uses the Tizen WebView browser control
	/// to render web content.
	/// </summary>
	public class TizenWebViewManager : WebViewManager
	{
		private const string AppOrigin = "http://0.0.0.0/";

		private readonly BlazorWebViewHandler _blazorMauiWebViewHandler;
		private readonly NWebView _webview;
		private readonly string _contentRootRelativeToAppRoot;

		/// <summary>
		/// Initializes a new instance of <see cref="TizenWebViewManager"/>
		/// </summary>
		/// <param name="blazorMauiWebViewHandler">The <see cref="BlazorWebViewHandler"/>.</param>
		/// <param name="webview">A wrapper to access platform-specific WebView APIs.</param>
		/// <param name="provider">The <see cref="IServiceProvider"/> for the application.</param>
		/// <param name="dispatcher">A <see cref="Dispatcher"/> instance instance that can marshal calls to the required thread or sync context.</param>
		/// <param name="fileProvider">Provides static content to the webview.</param>
		/// <param name="jsComponents">Describes configuration for adding, removing, and updating root components from JavaScript code.</param>
		/// <param name="contentRootRelativeToAppRoot">Path to the directory containing application content files.</param>
		/// <param name="hostPageRelativePath">Path to the host page within the fileProvider.</param>
		public TizenWebViewManager(BlazorWebViewHandler blazorMauiWebViewHandler, NWebView webview, IServiceProvider provider, Dispatcher dispatcher, IFileProvider fileProvider, JSComponentConfigurationStore jsComponents, string contentRootRelativeToAppRoot, string hostPageRelativePath)
			: base(provider, dispatcher, new Uri(AppOrigin), fileProvider, jsComponents, hostPageRelativePath)
		{
			_blazorMauiWebViewHandler = blazorMauiWebViewHandler ?? throw new ArgumentNullException(nameof(blazorMauiWebViewHandler));
			_webview = webview ?? throw new ArgumentNullException(nameof(webview));
			_contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;

		}

		internal bool TryGetResponseContentInternal(string uri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out IDictionary<string, string> headers)
		{
			var defaultResult = TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);
			var hotReloadedResult = StaticContentHotReloadManager.TryReplaceResponseContent(_contentRootRelativeToAppRoot, uri, ref statusCode, ref content, headers);
			return defaultResult || hotReloadedResult;
		}


		/// <inheritdoc />
		protected override void NavigateCore(Uri absoluteUri)
		{
			_webview.LoadUrl(absoluteUri.ToString());
		}

		/// <inheritdoc />
		protected override void SendMessage(string message)
		{
			var messageJSStringLiteral = JavaScriptEncoder.Default.Encode(message);
			_webview.EvaluateJavaScript($"__dispatchMessageCallback(\"{messageJSStringLiteral}\")");
		}

		internal void MessageReceivedInternal(Uri uri, string message)
		{
			MessageReceived(uri, message);
		}
	}
}
