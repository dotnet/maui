namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 26843, "WebView Fails to Load URLs with Certain Encoded Characters on Android", PlatformAffected.Android)]
	public partial class Issue26843 : ContentPage
	{
		private Label navigationResultLabel;
		public Issue26843()
		{
			navigationResultLabel = new Label
			{
				AutomationId = "NavigationResultLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Colors.Black,
				FontSize = 14
			};

			var webView = new WebView
			{
				AutomationId = "WebView",
				ZIndex = 1
			};

			webView.Source = "https://github.com/SuthiYuvaraj/maui/blob/390e4cd1c5eed59ecaf1fcd37975e7f6f5422d6d/src/Controls/tests/TestCases.HostApp/Resources/Raw/01-A%C4%9F-Sistem%20Bilgi%20G%C3%BCvenli%C4%9Fi%20Md/dotnet%20maui.pdf";
			webView.Navigated += OnNavigated;

			var layout = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
				},
				Children = {
					navigationResultLabel,
					webView
				}
			};

			Grid.SetRow(navigationResultLabel, 0);
			Grid.SetRow(webView, 1);

			Content = layout;
		}

		private void OnNavigated(object sender, WebNavigatedEventArgs e)
		{
			if (e.Result == WebNavigationResult.Success)
			{
				navigationResultLabel.Text = $"Successfully navigated to the encoded URL";
			}
			else
			{
				navigationResultLabel.Text = $"Failed to navigate to the encoded URL";
			}
		}
	}
