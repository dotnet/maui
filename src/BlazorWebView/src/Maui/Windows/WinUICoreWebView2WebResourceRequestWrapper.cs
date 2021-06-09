using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class WinUICoreWebView2WebResourceRequestWrapper : ICoreWebView2WebResourceRequestWrapper
	{
		private readonly CoreWebView2WebResourceRequestedEventArgs _webResourceRequestedEventArgs;

		public WinUICoreWebView2WebResourceRequestWrapper(CoreWebView2WebResourceRequestedEventArgs webResourceRequestedEventArgs)
		{
			_webResourceRequestedEventArgs = webResourceRequestedEventArgs;
		}

		public string Uri
		{
			get => _webResourceRequestedEventArgs.Request.Uri;
			set => _webResourceRequestedEventArgs.Request.Uri = value;
		}
		public string Method
		{
			get => _webResourceRequestedEventArgs.Request.Method;
			set => _webResourceRequestedEventArgs.Request.Method = value;
		}
	}
}
