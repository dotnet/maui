using System.Text;

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
			VerticalScrollBarVisibility = ScrollBarVisibility.Always,
			MaximumHeightRequest = 70,
			WidthRequest = 150
		};

		_scrollLabel = new Label
		{
			AutomationId = "ScrollLabel",
			Text = GenerateLabelText()
		};

		_scrollView.Content = _scrollLabel;
		mainGrid.Add(_scrollView, 0, 0);

		Content = mainGrid;
	}

	string GenerateLabelText()
	{
		var textBuilder = new StringBuilder();

		for (char c = 'a'; c <= 'z'; c++)
		{
			textBuilder.AppendLine(c.ToString());
		}

		return textBuilder.ToString();
	}

	void OnScrollButtonClicked(object sender, EventArgs e)
	{
		Application.Current?.Dispatcher.DispatchAsync(async () =>
		{
			await _scrollView.ScrollToAsync(0, 0, true);
			_scrollLabel.Text = "The text is successfully changed";
		});
	}
}

