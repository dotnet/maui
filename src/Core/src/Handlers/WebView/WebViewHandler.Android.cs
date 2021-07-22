using Android.Webkit;
using Android.Widget;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, AWebView>, IWebViewDelegate
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		WebViewClient? _webViewClient;
		WebChromeClient? _webChromeClient;

		protected override AWebView CreateNativeView()
		{
			var aWebView = new AWebView(Context!)
			{
#pragma warning disable 618 // This can probably be replaced with LinearLayout(LayoutParams.MatchParent, LayoutParams.MatchParent); just need to test that theory
				LayoutParameters = new AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0)
#pragma warning restore 618
			};

			if (aWebView.Settings != null)
			{
				aWebView.Settings.JavaScriptEnabled = true;
				aWebView.Settings.DomStorageEnabled = true;
			}

			_webViewClient = GetWebViewClient();
			aWebView.SetWebViewClient(_webViewClient);

			_webChromeClient = GetWebChromeClient();
			aWebView.SetWebChromeClient(_webChromeClient);

			return aWebView;
		}

		protected override void DisconnectHandler(AWebView nativeView)
		{
			nativeView.StopLoading();

			_webViewClient?.Dispose();
			_webChromeClient?.Dispose();
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate webViewDelegate = handler;

			handler.NativeView?.UpdateSource(webView, webViewDelegate);
		}
				
		public void LoadHtml(string? html, string? baseUrl)
		{
			NativeView?.LoadDataWithBaseURL(baseUrl ?? AssetBaseUrl, html ?? string.Empty, "text/html", "UTF-8", null);
		}

		public void LoadUrl(string? url)
		{
			NativeView?.LoadUrl(url ?? string.Empty);
		}

		protected virtual WebViewClient GetWebViewClient() =>
			new WebViewClient();

		protected virtual WebChromeClient GetWebChromeClient() =>
			new WebChromeClient();
	}
}