namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24831, "The BindingContext of the Window TitleBar is not being passed on to its child content", PlatformAffected.All)]
public class Issue24831 : TestContentPage
{
	TitleBar _customTitleBar;

	public string MainTitle { get; set; } = "Custom Title";
	public string SubTitle { get; set; } = "Custom Subtitle";
	public string SearchPlaceholder { get; set; } = "Search here...";
	public string ButtonText { get; set; } = "Settings";
	public string LeadingText { get; set; } = "Leading";


	protected override void Init()
	{

		BindingContext = this;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = 20,
				Children =
				{
					CreateLabel("This test verifies that TitleBar's BindingContext is properly propagated to Content, LeadingContent, and TrailingContent.", "descriptionLabel"),
				}
			}
		};

		CreateTitleBar();
	}

	Label CreateLabel(string text, string automationId = null, double fontSize = 16) =>
		new Label
		{
			Text = text,
			AutomationId = automationId,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			FontSize = fontSize,
			TextColor = Colors.Black,
			HorizontalTextAlignment = TextAlignment.Center
		};


	void CreateTitleBar()
	{
		// Content - SearchBar with binding
		var searchBar = new SearchBar
		{
			HorizontalOptions = LayoutOptions.FillAndExpand,
			MaximumWidthRequest = 300,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "TitleBarSearchBar"
		};
		searchBar.SetBinding(SearchBar.PlaceholderProperty, nameof(SearchPlaceholder));

		// Leading Content - Label with binding
		var leadingLabel = new Label
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			FontSize = 12,
			TextColor = Colors.White,
			AutomationId = "TitleBarLeadingLabel"
		};
		leadingLabel.SetBinding(Label.TextProperty, nameof(LeadingText));

		// Trailing Content - Button with binding
		var trailingButton = new Button
		{
			BorderWidth = 0,
			HeightRequest = 36,
			WidthRequest = 80,
			AutomationId = "TitleBarTrailingButton",
			FontSize = 10
		};
		trailingButton.SetBinding(Button.TextProperty, nameof(ButtonText));

		_customTitleBar = new TitleBar
		{
			HeightRequest = 48,
			BackgroundColor = Colors.DarkBlue,
			ForegroundColor = Colors.White,
			Content = searchBar,
			LeadingContent = leadingLabel,
			TrailingContent = trailingButton,
			AutomationId = "CustomTitleBar"
		};

		// Bind TitleBar properties
		_customTitleBar.SetBinding(TitleBar.TitleProperty, nameof(MainTitle));
		_customTitleBar.SetBinding(TitleBar.SubtitleProperty, nameof(SubTitle));
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		var window = Window ?? Shell.Current?.Window;
		if (window is not null)
		{
			window.BindingContext = this;
			window.TitleBar = _customTitleBar;
		}
	}
}