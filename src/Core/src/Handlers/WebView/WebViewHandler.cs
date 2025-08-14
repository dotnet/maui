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
#if __ANDROID__
			[nameof(WebViewClient)] = MapWebViewClient,
			[nameof(WebChromeClient)] = MapWebChromeClient,
			[nameof(WebView.Settings)] =  MapWebViewSettings
#elif __IOS__
			[nameof(WKUIDelegate)] = MapWKUIDelegate,
			[nameof(IWebView.Background)] = MapBackground,
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

		/// <summary>
		/// Creates a URI for cookie operations, with URL length truncation and validation.
		/// This method is shared across all platform implementations.
		/// </summary>
		/// <param name="url">The URL to convert to a URI</param>
		/// <returns>A valid Uri for cookie operations, or null if invalid</returns>
		internal static System.Uri? CreateUriForCookies(string? url)
		{
			if (url == null)
				return null;

			System.Uri? uri;

			if (url.Length > 2000)
				url = url.Substring(0, 2000);

			if (System.Uri.TryCreate(url, System.UriKind.Absolute, out uri))
			{
				if (string.IsNullOrWhiteSpace(uri.Host))
					return null;

				return uri;
			}

			return null;
		}
	}
}