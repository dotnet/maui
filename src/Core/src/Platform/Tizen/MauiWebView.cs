using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : NView, IWebViewDelegate
	{
		public MauiWebView()
		{
		}

		void IWebViewDelegate.LoadHtml(string? html, string? baseUrl)
		{
		}

		void IWebViewDelegate.LoadUrl(string? url)
		{
		}
	}
}