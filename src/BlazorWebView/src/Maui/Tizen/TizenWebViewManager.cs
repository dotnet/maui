using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class TizenWebViewManager : WebViewManager
	{
		private const string AppOrigin = "http://0.0.0.0/";

		private readonly BlazorWebViewHandler _blazorMauiWebViewHandler;
		private readonly TWebView _webview;

		public TizenWebViewManager(BlazorWebViewHandler blazorMauiWebViewHandler, TWebView webview, IServiceProvider services, Dispatcher dispatcher, IFileProvider fileProvider, JSComponentConfigurationStore jsComponents, string hostPageRelativePath)
			: base(services, dispatcher, new Uri(AppOrigin), fileProvider, jsComponents, hostPageRelativePath)
		{
			_blazorMauiWebViewHandler = blazorMauiWebViewHandler ?? throw new ArgumentNullException(nameof(blazorMauiWebViewHandler));
			_webview = webview ?? throw new ArgumentNullException(nameof(webview));

		}

		internal bool TryGetResponseContentInternal(string uri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out IDictionary<string, string> headers) =>
	TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);

		/// <inheritdoc />
		protected override void NavigateCore(Uri absoluteUri)
		{
			_webview.LoadUrl(absoluteUri.ToString());
		}

		/// <inheritdoc />
		protected override void SendMessage(string message)
		{
			var messageJSStringLiteral = JavaScriptEncoder.Default.Encode(message);
			_webview.Eval($"__dispatchMessageCallback(\"{messageJSStringLiteral}\")");
		}

		internal void MessageReceivedInternal(Uri uri, string message)
		{
			MessageReceived(uri, message);
		}
	}
}
