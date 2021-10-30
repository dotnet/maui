using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Web.WebView2.Core;
using Windows.Storage.Streams;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class WinUICoreWebView2WebResourceRequestedEventArgsWrapper : ICoreWebView2WebResourceRequestedEventArgsWrapper
	{
		private readonly CoreWebView2Environment _environment;
		private readonly CoreWebView2WebResourceRequestedEventArgs _webResourceRequestedEventArgs;

		public WinUICoreWebView2WebResourceRequestedEventArgsWrapper(CoreWebView2Environment environment, CoreWebView2WebResourceRequestedEventArgs webResourceRequestedEventArgs)
		{
			_environment = environment;
			_webResourceRequestedEventArgs = webResourceRequestedEventArgs;

			Request = new WinUICoreWebView2WebResourceRequestWrapper(webResourceRequestedEventArgs);
			ResourceContext = (CoreWebView2WebResourceContextWrapper)webResourceRequestedEventArgs.ResourceContext;
		}

		public ICoreWebView2WebResourceRequestWrapper Request { get; }

		public CoreWebView2WebResourceContextWrapper ResourceContext { get; }

		public IDeferralWrapper GetDeferral()
		{
			return new DeferralWrapper(_webResourceRequestedEventArgs.GetDeferral());
		}

		public void SetResponse(IRandomAccessStream content, int statusCode, string statusMessage, string headerString)
		{
			_webResourceRequestedEventArgs.Response = _environment.CreateWebResourceResponse(content, statusCode, statusMessage, headerString);
		}
	}
}
