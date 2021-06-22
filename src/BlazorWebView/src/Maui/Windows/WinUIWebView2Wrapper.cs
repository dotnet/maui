using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class WinUIWebView2Wrapper : IWebView2Wrapper
	{
		private readonly WinUICoreWebView2Wrapper _coreWebView2Wrapper;

		public WinUIWebView2Wrapper(WebView2Control webView2)
		{
			if (webView2 is null)
			{
				throw new ArgumentNullException(nameof(webView2));
			}

			WebView2 = webView2;
			_coreWebView2Wrapper = new WinUICoreWebView2Wrapper(this);
		}

		public ICoreWebView2Wrapper CoreWebView2 => _coreWebView2Wrapper;

		public Uri Source
		{
			get => WebView2.Source;
			set => WebView2.Source = value;
		}

		public WebView2Control WebView2 { get; }

		public CoreWebView2Environment? Environment { get; set; }

		public Action AddAcceleratorKeyPressedHandler(EventHandler<ICoreWebView2AcceleratorKeyPressedEventArgsWrapper> eventHandler)
		{
			// This event is not supported in WinUI, so we ignore it
			return () => { };
		}

		public async Task CreateEnvironmentAsync()
		{
			Environment = await CoreWebView2Environment.CreateAsync();
		}

		public Task EnsureCoreWebView2Async()
		{
			return WebView2.EnsureCoreWebView2Async().AsTask();
		}
	}
}
