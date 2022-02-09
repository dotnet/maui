using Android.Content;
using Android.Webkit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : WebView, IWebViewDelegate
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		public MauiWebView(Context context) : base(context)
		{
		}

		void IWebViewDelegate.LoadHtml(string? html, string? baseUrl)
		{
			LoadDataWithBaseURL(baseUrl ?? AssetBaseUrl, html ?? string.Empty, "text/html", "UTF-8", null);
		}

		void IWebViewDelegate.LoadUrl(string? url)
		{
			LoadUrl(url ?? string.Empty);
		}
	}
}
