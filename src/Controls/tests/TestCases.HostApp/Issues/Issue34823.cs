namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34823, "WebView on Windows Does Not Inherit App Theme", PlatformAffected.UWP)]
public class Issue34823 : NavigationPage
{
	public Issue34823() : base(new Issue34823MainPage())
	{
	}

	class Issue34823MainPage : ContentPage
	{
		public Issue34823MainPage()
		{
			BindingContext = this;
			Title = "WebView Test";
            
			var webButton = new Button
			{
				Text = "Switch to web page",
                AutomationId = "WebButton",
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(10)
			};
			webButton.Clicked += OnWebClicked;

			var themeButton = new Button
			{
				Text = "Toggle Theme",
				AutomationId = "ThemeButton",
				HorizontalOptions = LayoutOptions.Center
			};
			themeButton.Clicked += OnThemeButtonClicked;

			Content = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					Padding = 30,
					Spacing = 25,
					Children =
					{
						webButton,
						new HorizontalStackLayout
						{
							HorizontalOptions = LayoutOptions.Center,
							Children =
							{
								themeButton
							}
						}
					}
				}
			};
		}

		void OnThemeButtonClicked(object sender, EventArgs e)
		{
			Application.Current!.UserAppTheme = Dark ? AppTheme.Light : AppTheme.Dark;
		}

		async void OnWebClicked(object sender, EventArgs e)
		{
			var webView = new WebView();

			var helpPage = new ContentPage
			{
				Title = "Web Page",
				Content = new Grid
				{
					Children =
					{
						webView
					}
				}
			};
			helpPage.SetAppThemeColor(BackgroundColorProperty, Colors.White, Colors.Black);

			helpPage.NavigatedTo += async (_, __) =>
			{
				webView.Source = new HtmlWebViewSource
				{
					Html = """
						<html>
						<head>
						  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">
						  <title>Preparing Help</title>
						  <style>
						    @media (prefers-color-scheme: dark) {
						        html, body {
						            color: white;
						            background-color: black;
						        }
						    }

						    @media (prefers-color-scheme: light) {
						        html, body {
						            color: black;
						            background-color: white;
						        }
						    }
						  </style>
						</head>
						<body>
						<center><h1>Text on a web page</h1></center>
						</body>
						</html>
						"""
				};
			};

			await Navigation.PushAsync(helpPage);
		}

		public bool Dark
		{
			set
			{
				if (value != Dark)
				{
					Application.Current!.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
				}
			}
			get =>
				Application.Current?.UserAppTheme == AppTheme.Dark ||
				Application.Current?.RequestedTheme == AppTheme.Dark;
		}
	}
}