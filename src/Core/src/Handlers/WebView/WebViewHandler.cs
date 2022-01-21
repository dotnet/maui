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

		public static CommandMapper<IWebView, WebViewHandler> WebViewCommandMapper = new(ViewCommandMapper)
		{
			[nameof(IWebView.GoBack)] = MapGoBack,
			[nameof(IWebView.GoForward)] = MapGoForward,
			[nameof(IWebView.Reload)] = MapReload
		};

		public WebViewHandler() : base(WebViewMapper, WebViewCommandMapper)
		{

		}

		public WebViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? WebViewMapper, commandMapper ?? WebViewCommandMapper)
		{

		}
	}
}