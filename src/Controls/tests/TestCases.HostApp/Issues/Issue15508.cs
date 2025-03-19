namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 15508, "Scrollview.ScrollTo execution only returns after manual scroll", PlatformAffected.UWP)]
public class Issue15508 : ContentPage
{
	ScrollView _scrollView;
	Label _scrollLabel;
	Button _scrollButton;
	public Issue15508()
	{
		InitializeComponent();
	}

	void InitializeComponent()
	{
		var mainGrid = new Grid
		{
			ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star }
				},
			RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				},
			BackgroundColor = Colors.Gray
		};

		_scrollButton = new Button
		{
			Text = "Scroll activated through message",
			WidthRequest = 250,
			HorizontalOptions = LayoutOptions.Start,
			AutomationId = "ButtonToScroll"
		};

		_scrollButton.Clicked += OnScrollButtonClicked;

		var buttonLayout = new VerticalStackLayout();
		buttonLayout.Children.Add(_scrollButton);
		mainGrid.Add(buttonLayout, 1, 0);

		_scrollView = new ScrollView
		{
			BackgroundColor = Colors.LightCoral,
			VerticalOptions = LayoutOptions.Start,
			MaximumHeightRequest = 70,
			WidthRequest = 150
		};

		_scrollLabel = new Label
		{
			AutomationId = "ScrollLabel",
			Text = "Not Scrolled"
		};

		_scrollView.Content = _scrollLabel;
		mainGrid.Add(_scrollView, 0, 0);

		Content = mainGrid;
	}

	void OnScrollButtonClicked(object sender, EventArgs e)
	{
		Application.Current?.Dispatcher.DispatchAsync(async () =>
		{
			await _scrollView.ScrollToAsync(0, 0, true);
			_scrollLabel.Text = "Scroll Completed";
		});
	}
}

