using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
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

		public void SetResponse(Stream content, int statusCode, string statusMessage, string headerString)
		{
			// NOTE: This is stream copying is to work around a hanging bug in WinRT with managed streams
			var memStream = new MemoryStream();
			content.CopyTo(memStream);
			var ms = new InMemoryRandomAccessStream();
			ms.WriteAsync(memStream.GetWindowsRuntimeBuffer()).AsTask().Wait();

			_webResourceRequestedEventArgs.Response = _environment.CreateWebResourceResponse(ms, statusCode, statusMessage, headerString);
		}
	}
}
