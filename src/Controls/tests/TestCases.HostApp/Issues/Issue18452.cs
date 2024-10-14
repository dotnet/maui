using System.Net;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18452, "NullReferenceException throws on Windows when setting Cookies on .NET MAUI WebView", PlatformAffected.UWP)]
	public class Issue18452 : TestContentPage
	{
		protected override void Init()
		{
			bool isCookieSet = false;
			Grid grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.8, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.2, GridUnitType.Star) });
			WebView webView = new WebView();

			Label label = new Label();
			label.Text = "Test webview";
			label.AutomationId = "Label";

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


			grid.Children.Add(webView);

			webView.Navigated += (s, e) =>
			{
				var cookies = webView.Cookies.GetCookies(uri);
				foreach (Cookie c in cookies)
				{
					if (c.Name == "DotNetMAUICookie")
					{
						isCookieSet = true;
						grid.Children.Add(label);
						break;
					}
				}


				if (isCookieSet)
				{
					label.Text = "Success";
				}

			};

			Content = grid;
		}
	}
}