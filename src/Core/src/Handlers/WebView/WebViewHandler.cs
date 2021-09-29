#if __ANDROID__
using Android.Webkit;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler
	{
		public static PropertyMapper<IWebView, WebViewHandler> WebViewMapper = new PropertyMapper<IWebView, WebViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IWebView.Source)] = MapSource,
#if __ANDROID__
			[nameof(WebViewClient)] = MapWebViewClient,
			[nameof(WebChromeClient)] = MapWebChromeClient,
			[nameof(WebView.Settings)] =  MapWebViewSettings
#endif
		};

		public WebViewHandler() : base(WebViewMapper)
		{

		}

		public WebViewHandler(PropertyMapper mapper) : base(mapper ?? WebViewMapper)
		{

		}
	}
}