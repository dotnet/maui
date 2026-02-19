namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32586, "[iOS] Layout issue using TranslateToAsync causes infinite property changed cycle", PlatformAffected.iOS)]
public class Issue32586 : ContentPage
{
	const uint AnimationDuration = 250;
	Button FooterButton;
	Button ParentSafeAreaToggleButton;
	Button ChildSafeAreaToggleButton;
	Label TestLabel;
	Label SafeAreaStatusLabel;
	ContentView FooterView;
	Grid MainGrid;
	VerticalStackLayout verticalStackLayout;

	public Issue32586()
	{
		// Create the main grid
		MainGrid = new Grid
		{
			BackgroundColor = Colors.Orange,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};

		// Create the main content layout
		verticalStackLayout = new VerticalStackLayout
		{
			Padding = new Thickness(30, 10, 30, 0),
			Spacing = 25
		};

		// Top marker label - its Y position indicates whether safe area is applied
		var topMarker = new Label
		{
			Text = "Top Marker",
			FontSize = 12,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "TopMarker"
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

		// Create TestLabel
		TestLabel = new Label
		{
			Text = "Footer is not visible",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "TestLabel"
		};

		// Create SafeAreaEdges toggle button for parent Grid
		ParentSafeAreaToggleButton = new Button
		{
			Text = "Toggle Parent SafeArea",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "ParentSafeAreaToggleButton"
		};
		ParentSafeAreaToggleButton.Clicked += OnParentSafeAreaToggleClicked;

		// Create SafeAreaEdges toggle button for child verticalStackLayout
		ChildSafeAreaToggleButton = new Button
		{
			Text = "Toggle Child SafeArea",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "ChildSafeAreaToggleButton"
		};
		ChildSafeAreaToggleButton.Clicked += OnChildSafeAreaToggleClicked;

		// Create SafeAreaEdges status label
		SafeAreaStatusLabel = new Label
		{
			Text = "Parent: Container, Child: Container",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "SafeAreaStatusLabel"
		};

		// Add elements to stack layout
		verticalStackLayout.Add(topMarker);
		verticalStackLayout.Add(FooterButton);
		verticalStackLayout.Add(infoLabel);
		verticalStackLayout.Add(TestLabel);
		verticalStackLayout.Add(ParentSafeAreaToggleButton);
		verticalStackLayout.Add(ChildSafeAreaToggleButton);
		verticalStackLayout.Add(SafeAreaStatusLabel);

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
		Grid.SetRow(verticalStackLayout, 0);
		Grid.SetRow(FooterView, 1);
		MainGrid.Add(verticalStackLayout);
		MainGrid.Add(FooterView);

		// Set the grid as the page content
		Content = MainGrid;
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

	void OnParentSafeAreaToggleClicked(object sender, EventArgs e)
	{
		// Toggle SafeAreaEdges on the parent Grid between Container and None
		var currentEdges = MainGrid.SafeAreaEdges;
		if (currentEdges == new SafeAreaEdges(SafeAreaRegions.None))
		{
			MainGrid.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		}
		else
		{
			MainGrid.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None);
		}
		UpdateStatusLabel();
	}

	void OnChildSafeAreaToggleClicked(object sender, EventArgs e)
	{
		// Toggle SafeAreaEdges on the child verticalStackLayout between Container and None
		var currentEdges = verticalStackLayout.SafeAreaEdges;
		if (currentEdges == new SafeAreaEdges(SafeAreaRegions.None))
		{
			verticalStackLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		}
		else
		{
			verticalStackLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None);
		}
		UpdateStatusLabel();
	}

	void UpdateStatusLabel()
	{
		var parentEdges = MainGrid.SafeAreaEdges == new SafeAreaEdges(SafeAreaRegions.None) ? "None" : "Container";
		var childEdges = verticalStackLayout.SafeAreaEdges == new SafeAreaEdges(SafeAreaRegions.None) ? "None" : "Container";
		SafeAreaStatusLabel.Text = $"Parent: {parentEdges}, Child: {childEdges}";
	}
}