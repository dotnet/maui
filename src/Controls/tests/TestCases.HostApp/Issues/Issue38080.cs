namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38080, "Android SIGSEGV crash in GLFunctorDrawable when a ScrollView with an off-screen WebView is overscrolled", PlatformAffected.Android)]
public class Issue38080 : NavigationPage
{
	public Issue38080() : base(new Issue38080HomePage()) { }

	class Issue38080HomePage : ContentPage
	{
		public Issue38080HomePage()
		{
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						AutomationId = "Issue38080NavigateButton",
						Text = "Open repro page",
						Command = new Command(async () => await Navigation.PushAsync(new Issue38080ReproPage()))
					}
				}
			};
		}
	}

	// Reproduces #38080: a single fixed-size WebView placed mid-list in a ScrollView is off-screen
	// at both scroll extremes. Flinging to either extreme (or back-navigating) previously SIGSEGV'd
	// the RenderThread in GLFunctorDrawable because the WebView carried a non-null ClipBounds that
	// routed its off-screen compositing through the AOSP hwui GL-functor path.
	class Issue38080ReproPage : ContentPage
	{
		const int FillerRows = 50;

		public Issue38080ReproPage()
		{
			var stack = new VerticalStackLayout
			{
				Padding = new Thickness(12),
				Spacing = 12
			};

			stack.Children.Add(new Label
			{
				AutomationId = "Issue38080Ready",
				Text = "Page loaded — no crash"
			});

			AddFiller(stack, FillerRows);

			// Single fixed-size WebView mid-list, so it is off-screen at both extremes.
			stack.Children.Add(new WebView
			{
				HeightRequest = 220,
				Source = new HtmlWebViewSource
				{
					Html = "<html><body style='background:#d6e4ff'><h3>WebView</h3></body></html>"
				}
			});

			AddFiller(stack, FillerRows);

			Content = new ScrollView
			{
				AutomationId = "Issue38080ScrollView",
				Content = stack
			};
		}

		static void AddFiller(VerticalStackLayout stack, int rows)
		{
			for (int i = 1; i <= rows; i++)
			{
				stack.Children.Add(new Label { Text = $"Filler row {i}", FontSize = 20 });
			}
		}
	}
}
