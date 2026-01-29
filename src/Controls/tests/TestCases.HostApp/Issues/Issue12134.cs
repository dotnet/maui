using System.Net;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 12134, "[iOS] WkWebView does not handle cookies consistently",
		PlatformAffected.iOS, isInternetRequired: true)]
	public class Issue12134 : TestContentPage
	{
		Button button;
		Guid _guid = Guid.NewGuid();
		Label _label = new Label();
		protected override void Init()
		{
			WebView webView = new WebView()
			{
				HeightRequest = 400
			};

			button = new Button()
			{
				Text = "Load another webview",
				Command = new Command(() =>
				{
					OnButtonClicked(this, EventArgs.Empty);
				}),
				AutomationId = "LoadNewWebView"
			};

			Content = new StackLayout()
			{
				Children =
				{
					GetWebView(),
					_label,
					new Button(){ Text = "Display Cookies", Command = new Command(DisplayCookies) },
					button,
				}
			};

		}

		private async void DisplayCookies()
		{
			var result = await (Content as StackLayout).Children.OfType<WebView>().Last().EvaluateJavaScriptAsync("document.cookie");
			await DisplayAlertAsync("Cookies", result, "Ok");
		}

		private async void WebViewOnNavigated(object sender, WebNavigatedEventArgs e)
		{
			var result = await ((WebView)sender).EvaluateJavaScriptAsync("document.cookie");
			_label.Text = result.Contains(_guid.ToString(), StringComparison.OrdinalIgnoreCase) ? "Success" : "Failed";
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			_label.Text = "";
			if (GetWebViews().Length >= 2)
			{
				foreach (var wv in GetWebViews())
					(Content as StackLayout).Children.Remove(wv);
				(Content as StackLayout).Children.Insert(0, GetWebView());
			}
			else
			{

				(Content as StackLayout).Children.Add(GetWebView());
				button.Text = "Reload the page";
			}
		}

		WebView[] GetWebViews() => (Content as StackLayout).Children.OfType<WebView>().ToArray();

		private Cookie GetTestCookie()
		{
			return new Cookie("TestCookie", $"{_guid}", "/", "dotnet.microsoft.com");
		}

		private WebView GetWebView()
		{
			var anotherWebView = new WebView
			{
				HeightRequest = 400
			};

			SetCookieContainer(anotherWebView);
			anotherWebView.Navigated += WebViewOnNavigated;
			anotherWebView.Source = "https://dotnet.microsoft.com";
			return anotherWebView;
		}

		private void SetCookieContainer(WebView wv)
		{
			wv.Cookies = new CookieContainer();
			wv.Cookies.Add(GetTestCookie());
		}
	}
}
