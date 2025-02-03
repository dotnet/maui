using System.Net;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18452, "NullReferenceException throws on Windows when setting Cookies on .NET MAUI WebView", PlatformAffected.UWP)]
	public class Issue18452 : TestContentPage
	{
		protected override void Init()
		{
			Grid grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.8, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.2, GridUnitType.Star) });
			WebView webView = new WebView();

			Label label = new Label();
			label.AutomationId = "Label";

			const string url = "https://learn.microsoft.com/en-us/dotnet/";

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
						if (!grid.Contains(label))
						{
							grid.Children.Add(label);
							label.Text = "Success";
							break;
						}
					}
				}
			};

			Content = grid;
		}
	}
}
