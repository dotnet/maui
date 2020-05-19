using System;

using System.Maui;

namespace System.Maui.Controls
{
	public class WebsiteView : BaseView
	{
		public WebsiteView(string site, string title)
		{
			this.Title = title;
			var webView = new WebView();
			webView.Source = new UrlWebViewSource
			{
				Url = site
			};
			Content = webView;
		}
	}
}

