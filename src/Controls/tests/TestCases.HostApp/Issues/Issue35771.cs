namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35771, "Android SIGSEGV crash with multiple auto-sizing WebViews in ScrollView on navigated page", PlatformAffected.Android)]
public class Issue35771 : NavigationPage
{
	public Issue35771() : base(new Issue35771HomePage()) { }

	class Issue35771HomePage : ContentPage
	{
		public Issue35771HomePage()
		{
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						AutomationId = "Issue35771NavigateButton",
						Text = "Open repro page",
						Command = new Command(async () => await Navigation.PushAsync(new Issue35771ReproPage()))
					},
					new Button
					{
						AutomationId = "Issue35771HorizontalNavigateButton",
						Text = "Open horizontal repro page",
						Command = new Command(async () => await Navigation.PushAsync(new Issue35771HorizontalReproPage()))
					},
					new Button
					{
						AutomationId = "Issue35771PopAsyncNavigateButton",
						Text = "Open PopAsync repro page",
						Command = new Command(async () => await Navigation.PushAsync(new Issue35771PopAsyncReproPage()))
					}
				}
			};
		}
	}

	class Issue35771ReproPage : ContentPage
	{
		public Issue35771ReproPage()
		{
			var stack = new VerticalStackLayout
			{
				Children =
				{
					new Label
					{
						AutomationId = "Issue35771Ready",
						Text = "Page loaded — no crash"
					}
				}
			};

			for (int i = 1; i <= 6; i++)
			{
				stack.Children.Add(new WebView
				{
					Source = new HtmlWebViewSource { Html = $"<html><body><p>WebView {i}</p></body></html>" }
				});
			}

			Content = new ScrollView { Content = stack };
		}
	}

	// Reproduces the horizontal auto-sizing crash scenario (width == 0, height > 0):
	// WebViews inside a HorizontalStackLayout have a fixed HeightRequest but no WidthRequest,
	// so the first layout pass produces (w=0, h>0) on the native view.
	class Issue35771HorizontalReproPage : ContentPage
	{
		public Issue35771HorizontalReproPage()
		{
			var stack = new HorizontalStackLayout();

			for (int i = 1; i <= 6; i++)
			{
				stack.Children.Add(new WebView
				{
					HeightRequest = 200,
					Source = new HtmlWebViewSource { Html = $"<html><body><p>WebView {i}</p></body></html>" }
				});
			}

			Content = new ScrollView
			{
				Orientation = ScrollOrientation.Horizontal,
				Content = new VerticalStackLayout
				{
					Children =
					{
						new Label
						{
							AutomationId = "Issue35771HorizontalReady",
							Text = "Horizontal page loaded — no crash"
						},
						stack
					}
				}
			};
		}
	}

	// Reproduces the PopAsync crash scenario: popping a page while WebViews are still in
	// the dangerous (w>0, h=0) first-layout state previously caused a RenderThread SIGSEGV.
	class Issue35771PopAsyncReproPage : ContentPage
	{
		public Issue35771PopAsyncReproPage()
		{
			var stack = new VerticalStackLayout
			{
				Children =
				{
					new Label
					{
						AutomationId = "Issue35771PopAsyncReady",
						Text = "Page loaded — tap button to pop"
					},
					new Button
					{
						AutomationId = "Issue35771PopAsyncPopButton",
						Text = "Pop back",
						Command = new Command(async () => await Navigation.PopAsync())
					}
				}
			};

			for (int i = 1; i <= 6; i++)
			{
				stack.Children.Add(new WebView
				{
					Source = new HtmlWebViewSource { Html = $"<html><body><p>WebView {i}</p></body></html>" }
				});
			}

			Content = new ScrollView { Content = stack };
		}
	}
}
