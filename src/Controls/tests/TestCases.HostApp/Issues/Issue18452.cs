using System.Net;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18452, "NullReferenceException throws on Windows when setting Cookies on .NET MAUI WebView", PlatformAffected.UWP)]
	public class Issue18452 : TestContentPage
	{
		protected override void Init()
		{
			VerticalStackLayout stackLayout = new VerticalStackLayout();
			WebView webView = new WebView();
			const string url = "https://httpbin.org/#/Cookies/get_cookies";

			CookieContainer cookieContainer = new();
			Uri uri = new(url, UriKind.RelativeOrAbsolute);

			Cookie cookie = new()
			{
				Name = "DotNetMAUICookie",
				Expires = DateTime.Now.AddDays(1),
				Value = "My cookie",
				Domain = uri.Host,
				Path = "/"
			};

			cookieContainer.Add(uri, cookie);
			webView.Cookies = cookieContainer;
			webView.AutomationId = "WebView";
			webView.Source = new UrlWebViewSource { Url = uri.ToString() };
			Label label = new Label()
			{
				AutomationId = "Label",
				Text = "This is a test label"
			};
			
			stackLayout.Children.Add(webView);
			stackLayout.Children.Add(label);
			Content = stackLayout;
		}
	}
}