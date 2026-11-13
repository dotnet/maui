namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25599_1, "Shell Navigating event shows identical Current and Target on tab click", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue25599_1 : Shell
{
	public Issue25599_1()
	{
		var tabBar = new TabBar();

		var homeTab = new Tab
		{
			Title = "Home"
		};
		var homeShellContent = new ShellContent
		{
			ContentTemplate = new DataTemplate(() => new Issue25599_1MainPage()),
			Route = "MainPage"
		};
		homeTab.Items.Add(homeShellContent);
		tabBar.Items.Add(homeTab);

		var settingsTab = new Tab
		{
			Title = "Settings"
		};
		var settingsShellContent = new ShellContent
		{
			ContentTemplate = new DataTemplate(() => new Issue25599_1DetailPage()),
			Route = "DetailPage"
		};
		settingsTab.Items.Add(settingsShellContent);
		tabBar.Items.Add(settingsTab);
		Items.Add(tabBar);
	}
}

public class Issue25599_1MainPage : ContentPage
{
	Label navigatedLabel;
	Label navigatingCurrentLabel;
	Label navigatingTargetLabel;

	public Issue25599_1MainPage()
	{
		var layout = new StackLayout
		{
			Padding = 20,
			Spacing = 10
		};

		var button = new Button
		{
			Text = "Push Detail Page",
			AutomationId = "PushButton"
		};
		button.Clicked += async (s, e) =>
		{
			await Navigation.PushAsync(new Issue25599_1DetailPage());
		};

		navigatingCurrentLabel = new Label
		{
			Text = "Navigating Current: Not triggered",
			AutomationId = "MainPageNavigatingCurrentLabel",
			FontSize = 12,
			LineBreakMode = LineBreakMode.WordWrap
		};

		navigatingTargetLabel = new Label
		{
			Text = "Navigating Target: Not triggered",
			AutomationId = "MainPageNavigatingTargetLabel",
			FontSize = 12,
			LineBreakMode = LineBreakMode.WordWrap
		};

		navigatedLabel = new Label
		{
			Text = "Navigated: Not triggered",
			AutomationId = "MainPageNavigatedLabel",
			FontSize = 12,
			LineBreakMode = LineBreakMode.WordWrap
		};

		layout.Add(button);
		layout.Add(navigatingCurrentLabel);
		layout.Add(navigatingTargetLabel);
		layout.Add(navigatedLabel);

		Content = layout;
		Title = "MainPage";

		if (Application.Current?.MainPage is Issue25599_1 shell)
		{
			shell.Navigating += OnNavigating;
			shell.Navigated += OnNavigated;
		}
	}

	void OnNavigating(object sender, ShellNavigatingEventArgs e)
	{
		navigatingCurrentLabel.Text = $"{e.Current?.Location}";
		navigatingTargetLabel.Text = $"{e.Target?.Location}";
	}

	void OnNavigated(object sender, ShellNavigatedEventArgs e)
	{
		navigatedLabel.Text = $"Navigated:\nPrevious: {e.Previous?.Location}\nCurrent: {e.Current?.Location}";
	}
}

public class Issue25599_1DetailPage : ContentPage
{
	public Issue25599_1DetailPage()
	{
		var layout = new StackLayout
		{
			Padding = 20,
			Spacing = 10,
			HeightRequest = 100
		};

		var popButton = new Button
		{
			Text = "Pop",
			AutomationId = "PopButton"
		};

		layout.Add(popButton);
		Content = layout;
		Title = "DetailPage";

	}
}
