using NWebView = Tizen.NUI.BaseComponents.WebView;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : NWebView, IWebViewDelegate
	{
		public MauiWebView()
		{
			MouseEventsEnabled = true;
			KeyEventsEnabled = true;
			EnableJavaScript = true;
		}

		void IWebViewDelegate.LoadHtml(string? html, string? baseUrl)
		{
			if (baseUrl != null)
			{
				LoadContents(html, (uint)(html?.Length ?? 0), "text/html", "UTF-8", baseUrl);
			}
			else
			{
				LoadHtmlString(html);
			}
		}

		void IWebViewDelegate.LoadUrl(string? url)
		{
			LoadUrl(url);
		}
	}
}