#if ANDROID
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38080, "Android WebView crashes during ScrollView overscroll", PlatformAffected.Android)]
public class Issue38080 : NavigationPage
{
	const string Html = "<html><body style='background:#d6e4ff'><h3>WebView</h3></body></html>";

	public Issue38080() : base(new Issue38080HomePage())
	{
	}

	sealed class Issue38080HomePage : ContentPage
	{
		public Issue38080HomePage()
		{
			var navigateButton = new Button
			{
				AutomationId = "Issue38080NavigateButton",
				Text = "Open WebView overscroll repro"
			};
			navigateButton.Clicked += async (_, _) => await Navigation.PushAsync(CreateReproPage());

			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						AutomationId = "Issue38080HomeMarker",
						Text = "Issue 38080 home"
					},
					navigateButton
				}
			};
		}
	}

	static ContentPage CreateReproPage()
	{
		var content = new VerticalStackLayout
		{
			Padding = 12,
			Spacing = 12
		};

		var topMarker = CreateFillerRow(1, null);
		content.Children.Add(topMarker);

		for (var row = 2; row <= 50; row++)
		{
			content.Children.Add(CreateFillerRow(row, null));
		}

		var webView = new WebView
		{
			AutomationId = "Issue38080WebView",
			HeightRequest = 220,
			Source = new HtmlWebViewSource { Html = Html }
		};
		var navigationCompleted = false;

		void UpdateWebViewStatus()
		{
			var platformView = webView.Handler?.PlatformView as Android.Webkit.WebView;
			Android.Util.Log.Info(
				"Issue38080",
				$"Loaded:{navigationCompleted};Width:{platformView?.Width ?? 0};Height:{platformView?.Height ?? 0};" +
				$"Attached:{platformView?.IsAttachedToWindow == true};Hardware:{platformView?.IsHardwareAccelerated == true}");

			if (topMarker.AutomationId is null &&
				navigationCompleted &&
				platformView is { Width: > 0, Height: > 0, IsAttachedToWindow: true, IsHardwareAccelerated: true })
			{
				topMarker.AutomationId = "Issue38080TopMarker";
			}
		}

		webView.Loaded += (_, _) => UpdateWebViewStatus();
		webView.SizeChanged += (_, _) => UpdateWebViewStatus();
		webView.Navigated += (_, _) =>
		{
			navigationCompleted = true;
			UpdateWebViewStatus();
		};

		content.Children.Add(webView);

		for (var row = 51; row <= 100; row++)
		{
			content.Children.Add(CreateFillerRow(row,
				row == 100 ? "Issue38080BottomMarker" : null));
		}

		return new ContentPage
		{
			Title = "Issue 38080",
			Content = new ScrollView
			{
				AutomationId = "Issue38080ReproPageMarker",
				Content = content
			}
		};
	}

	static Label CreateFillerRow(int row, string automationId)
	{
		return new Label
		{
			AutomationId = automationId,
			FontSize = 20,
			Text = $"Filler row {row}"
		};
	}
}
#endif
