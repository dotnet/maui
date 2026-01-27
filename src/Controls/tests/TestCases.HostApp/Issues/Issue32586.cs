namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32586, "[iOS] Layout issue using TranslateToAsync causes infinite property changed cycle", PlatformAffected.iOS)]
public class Issue32586 : ContentPage
{
	const uint AnimationDuration = 250;
	Button FooterButton;
	Label TestLabel;
	ContentView FooterView;

	public Issue32586()
	{
		// Create the main grid
		var grid = new Grid
		{
			BackgroundColor = Colors.Orange,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};

		// Create the main content layout
		var stackLayout = new VerticalStackLayout
		{
			Padding = new Thickness(30, 10, 30, 0),
			Spacing = 25
		};

		// Create FooterButton
		FooterButton = new Button
		{
			Text = "Show Footer",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "FooterButton"
		};
		FooterButton.Clicked += OnFooterButtonClicked;

		// Create info label
		var infoLabel = new Label
		{
			Text = "Click to verify UI responsiveness",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "InfoLabel"
		};

		// Create TestButton
		TestLabel = new Label
		{
			Text = "Footer is not visible",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "TestLabel"
		};

		// Add elements to stack layout
		stackLayout.Add(FooterButton);
		stackLayout.Add(infoLabel);
		stackLayout.Add(TestLabel);

		// Create the footer view
		FooterView = new ContentView
		{
			IsVisible = false,
			AutomationId = "FooterView"
		};

		// Create the footer grid content
		var footerGrid = new Grid();

		// Create the gradient background
		var gradientBoxView = new BoxView
		{
			BackgroundColor = Colors.Transparent,
			Opacity = 1.0,
			VerticalOptions = LayoutOptions.Fill,
			Background = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(0, 1),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Transparent, Offset = 0.0f },
					new GradientStop { Color = Colors.Black, Offset = 1.0f }
				}
			}
		};

		// Create the footer button
		var footerButton = new Button
		{
			Text = "I am the footer",
			BackgroundColor = Colors.LightGray,
			Padding = new Thickness(10),
			AutomationId = "FooterContentButton"
		};

		// Add elements to footer grid
		footerGrid.Add(gradientBoxView);
		footerGrid.Add(footerButton);

		// Set footer grid as content of footer view
		FooterView.Content = footerGrid;

		// Add elements to main grid
		Grid.SetRow(stackLayout, 0);
		Grid.SetRow(FooterView, 1);
		grid.Add(stackLayout);
		grid.Add(FooterView);

		// Set the grid as the page content
		Content = grid;
	}

	void OnFooterButtonClicked(object sender, EventArgs e)
	{
		if (!FooterView.IsVisible)
		{
			Dispatcher.DispatchAsync(ShowFooter);
		}
		else
		{
			Dispatcher.DispatchAsync(HideFooter);
		}
	}

	async Task ShowFooter()
	{
		if (FooterView.IsVisible)
		{
			return;
		}

		var height = FooterView.Measure(FooterView.Width, double.PositiveInfinity).Height;
		FooterView.TranslationY = height;
		FooterView.IsVisible = true;

		// This causes deadlock on iOS .NET 10
		await FooterView.TranslateToAsync(0, 0, AnimationDuration, Easing.CubicInOut);

		TestLabel.Text = "Footer is now visible";
	}

	async Task HideFooter()
	{
		if (!FooterView.IsVisible)
		{
			return;
		}

		await FooterView.TranslateToAsync(0, FooterView.Height, AnimationDuration, Easing.CubicInOut);
		FooterView.IsVisible = false;

		TestLabel.Text = "Footer is now hidden";
	}
}