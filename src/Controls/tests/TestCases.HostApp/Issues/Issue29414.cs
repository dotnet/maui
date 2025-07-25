namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29414, "[Android] Loaded event not triggered when navigating back to a previous page", PlatformAffected.Android)]
public partial class Issue29414 : ContentPage
{
	private int _mainPageLoadedCount = 0;
	private Label _mainPageLoadedLabel;
	private Label _secondPageLoadedLabel;

	public Issue29414()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		var layout = new StackLayout
		{
			Spacing = 10,
			Padding = 20
		};

		var titleLabel = new Label
		{
			Text = "Main Page - Loaded Event Test",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			AutomationId = "MainPageTitle"
		};

		_mainPageLoadedLabel = new Label
		{
			Text = "Main Page Loaded Count: 0",
			AutomationId = "MainPageLoadedCount"
		};

		var navigateButton = new Button
		{
			Text = "Navigate to Second Page",
			AutomationId = "NavigateToSecondPageButton"
		};
		navigateButton.Clicked += OnNavigateToSecondPageClicked;

		layout.Children.Add(titleLabel);
		layout.Children.Add(_mainPageLoadedLabel);
		layout.Children.Add(navigateButton);

		Content = layout;

		// Subscribe to the Loaded event
		Loaded += OnMainPageLoaded;
	}

	private void OnMainPageLoaded(object sender, EventArgs e)
	{
		_mainPageLoadedCount++;
		_mainPageLoadedLabel.Text = $"Main Page Loaded Count: {_mainPageLoadedCount}";
	}

	private async void OnNavigateToSecondPageClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SecondPage());
	}
}

public partial class SecondPage : ContentPage
{
	private int _secondPageLoadedCount = 0;
	private Label _secondPageLoadedLabel;

	public SecondPage()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		var layout = new StackLayout
		{
			Spacing = 10,
			Padding = 20
		};

		var titleLabel = new Label
		{
			Text = "Second Page",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			AutomationId = "SecondPageTitle"
		};

		_secondPageLoadedLabel = new Label
		{
			Text = "Second Page Loaded Count: 0",
			AutomationId = "SecondPageLoadedCount"
		};

		var navigateBackButton = new Button
		{
			Text = "Navigate Back to Main Page",
			AutomationId = "NavigateBackToMainPageButton"
		};
		navigateBackButton.Clicked += OnNavigateBackClicked;

		layout.Children.Add(titleLabel);
		layout.Children.Add(_secondPageLoadedLabel);
		layout.Children.Add(navigateBackButton);

		Content = layout;

		// Subscribe to the Loaded event
		Loaded += OnSecondPageLoaded;
	}

	private void OnSecondPageLoaded(object sender, EventArgs e)
	{
		_secondPageLoadedCount++;
		_secondPageLoadedLabel.Text = $"Second Page Loaded Count: {_secondPageLoadedCount}";
	}

	private async void OnNavigateBackClicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
}