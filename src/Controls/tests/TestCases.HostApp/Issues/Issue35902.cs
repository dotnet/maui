namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35902, "[iOS] Transparent Shell Navigation Bar Breaks After Keyboard Interaction on Secondary Pages", PlatformAffected.iOS)]
public class Issue35902 : TestShell
{
	public Issue35902()
	{
		BackgroundColor = Colors.Transparent;
		Routing.RegisterRoute(nameof(Issue35902SecondPage), typeof(Issue35902SecondPage));
	}

	protected override void Init()
	{
		AddContentPage(new Issue35902MainPage());
	}
}

public class Issue35902MainPage : ContentPage
{
	public Issue35902MainPage()
	{
		Title = "Issue 35902";

		var navigateButton = new Button
		{
			Text = "Navigate to Second Page",
			AutomationId = "NavigateButton",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		navigateButton.Clicked += async (s, e) =>
		{
			await Shell.Current.GoToAsync(nameof(Issue35902SecondPage));
		};

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children = { navigateButton }
		};
	}
}

public class Issue35902SecondPage : ContentPage
{
	public Issue35902SecondPage()
	{
		Title = "Second Page";
		BackgroundColor = Colors.LightSkyBlue;

		var entry = new Entry
		{
			Placeholder = "Tap here to show keyboard",
			AutomationId = "TestEntry"
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			VerticalOptions = LayoutOptions.End,
			Children =
			{
				new Label { Text = "Tap Entry, then dismiss keyboard" },
				entry
			}
		};
	}
}
