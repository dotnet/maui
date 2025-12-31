using System.Net;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18452, "NullReferenceException throws on Windows when setting Cookies on .NET MAUI WebView", PlatformAffected.UWP, isInternetRequired: true)]
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

			const string url = "https://learn.microsoft.com";

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

			webView.Navigated += async (s, e) =>
			{
#if ANDROID
				await Task.Delay(300);
				var cookieString = Android.Webkit.CookieManager.Instance?.GetCookie("https://learn.microsoft.com");
			
				if (!string.IsNullOrEmpty(cookieString) && cookieString.Contains("DotNetMAUICookie", StringComparison.OrdinalIgnoreCase))
				{
					if (!grid.Contains(label))
					{
						grid.Children.Add(label);
						label.Text = "Success";
					}
				}
#else
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
#endif
			};

			Content = grid;
		}
	}
}
