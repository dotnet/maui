using System.Net;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18452, "NullReferenceException throws on Windows when setting Cookies on .NET MAUI WebView", PlatformAffected.UWP)]
	public partial class Issue18452 : ContentPage
	{
		public Issue18452()
		{
			InitializeComponent();

			const string url = "https://www.google.com";

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
			webView.Source = new UrlWebViewSource { Url = uri.ToString() };
		}
	}
}