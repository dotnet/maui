namespace Microsoft.Maui.Controls
{
	public partial class WebView : IWebView
	{
		bool _canGoBack;
		bool _canGoForward;

		IWebViewSource IWebView.Source => Source;
		
		bool IWebView.CanGoBack
		{
			get => _canGoBack;
			set
			{
				_canGoBack = value;
				((IWebViewController)this).CanGoBack = _canGoBack;
				Handler?.UpdateValue(nameof(IWebView.CanGoBack));
			}
		}

		bool IWebView.CanGoForward
		{
			get => _canGoForward;
			set
			{
				_canGoForward = value;
				((IWebViewController)this).CanGoForward = _canGoForward;
				Handler?.UpdateValue(nameof(IWebView.CanGoForward));
			}
		}
	}
}