#if __IOS__ || MACCATALYST
using PlatformView = WebKit.WKWebView;
#elif MONOANDROID
using PlatformView = Android.Webkit.WebView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.WebView2;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiWebView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

#if __ANDROID__
using Android.Webkit;
#elif __IOS__
using WebKit;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : IWebViewHandler
	{
		public static IPropertyMapper<IWebView, IWebViewHandler> Mapper = new PropertyMapper<IWebView, IWebViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IWebView.Source)] = MapSource,
			[nameof(IWebView.UserAgent)] = MapUserAgent,
#if WINDOWS
	[nameof(IView.FlowDirection)] = MapFlowDirection,
#endif
#if __ANDROID__
			[nameof(WebViewClient)] = MapWebViewClient,
			[nameof(WebChromeClient)] = MapWebChromeClient,
			[nameof(WebView.Settings)] =  MapWebViewSettings
#elif __IOS__
			[nameof(WKUIDelegate)] = MapWKUIDelegate,
			[nameof(IWebView.Background)] = MapBackground,
			[nameof(IWebView.FlowDirection)] = MapFlowDirection,
#endif
		};

		public static CommandMapper<IWebView, IWebViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IWebView.GoBack)] = MapGoBack,
			[nameof(IWebView.GoForward)] = MapGoForward,
			[nameof(IWebView.Reload)] = MapReload,
			[nameof(IWebView.Eval)] = MapEval,
			[nameof(IWebView.EvaluateJavaScriptAsync)] = MapEvaluateJavaScriptAsync,
		};

		public WebViewHandler() : base(Mapper, CommandMapper)
		{
		}

		public WebViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IWebView IWebViewHandler.VirtualView => VirtualView;

		PlatformView IWebViewHandler.PlatformView => PlatformView;
	}
}