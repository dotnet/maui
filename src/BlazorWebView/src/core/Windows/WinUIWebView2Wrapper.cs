using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class WinUIWebView2Wrapper : IWebView2Wrapper
	{
		private readonly WebView2Control _webView2;
		private bool _hasInitialized;

		public WinUIWebView2Wrapper(WebView2Control webView2)
		{
			_webView2 = webView2 ?? throw new ArgumentNullException(nameof(webView2));
		}

		public CoreWebView2 CoreWebView2 => _webView2.CoreWebView2;

		public Uri Source
		{
			get => _webView2.Source;
			set => _webView2.Source = value;
		}

		public event EventHandler<CoreWebView2AcceleratorKeyPressedEventArgs> AcceleratorKeyPressed
		{
			add => throw new NotSupportedException(); //_webView2.AcceleratorKeyPressed += value;
			remove => throw new NotSupportedException(); //_webView2.AcceleratorKeyPressed -= value;
		}

		public Task EnsureCoreWebView2Async(CoreWebView2Environment? environment = null)
		{
			if (_hasInitialized)
			{
				// We don't want people to think they can set more than one environment
				throw new InvalidOperationException($"{nameof(EnsureCoreWebView2Async)} may only be called once per control.");
			}

			_hasInitialized = true;
			// TODO: We don't pass in the 'environment' parameter here because it seems WinUI doesn't support that
			return _webView2.EnsureCoreWebView2Async().AsTask();
		}
	}
}
