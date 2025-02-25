namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6286, "ObjectDisposedException in Android WebView.EvaluateJavascriptAsync", PlatformAffected.Android, isInternetRequired: true)]
public class Issue6286 : TestNavigationPage
{
	WebView _webview;
	WebView _webview2;
	ContentPage _page1;
	ContentPage _page2;

	protected override void Init()
	{
		_webview = new WebView { Source = "https://microsoft.com" };
		_webview.Navigated += OnWebviewNavigated;
		_page1 = new ContentPage { Content = _webview };

		_webview2 = new WebView { Source = "https://google.com" };
		_webview2.Navigated += OnWebviewNavigated;
		_page2 = new ContentPage { Content = _webview2 };

		Navigation.PushAsync(_page1);
		RunTest();
	}

	async void RunTest()
	{
		try
		{
			int count = 0;
			while (count < 3)
			{
				await Task.Delay(2000);
				count++;

				_webview.Source = "https://google.com";
				await Navigation.PushAsync(_page2);

				_webview2.Source = "https://microsoft.com";
				await Navigation.PopAsync();
			}

			_page1.Content = new Label { Text = "success", AutomationId = "success" };
		}
		catch (Exception exc)
		{
			_page1.Content = new Label { Text = $"{exc}", AutomationId = "failure" };
		}
	}

	async void OnWebviewNavigated(object sender, WebNavigatedEventArgs e)
	{
		await _webview.EvaluateJavaScriptAsync("document.write('i executed this javascript woohoo');");

		await _webview2.EvaluateJavaScriptAsync("document.write('i executed this javascript woohoo');");
	}
}
