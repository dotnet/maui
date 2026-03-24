namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34584, "Content renders under status bar when navigating with keyboard open to a page with NavBarIsVisible=False", PlatformAffected.Android)]
public class Issue34584 : TestShell
{
	protected override void Init()
	{
		var mainPage = new Issue34584_MainPage();
		var destinationPage = new Issue34584_DestinationPage();

		AddContentPage(mainPage, "MainPage");
		AddContentPage(destinationPage, "DestinationPage");
	}
}

public class Issue34584_MainPage : ContentPage
{
	public Issue34584_MainPage()
	{
		var entry = new Entry
		{
			Placeholder = "Tap here to open keyboard",
			AutomationId = "Entry"
		};

		var navigateButton = new Button
		{
			Text = "Navigate",
			AutomationId = "NavigateButton"
		};

		entry.Completed += async (s, e) =>
		{
			await Shell.Current.GoToAsync("//DestinationPage", false);
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(0),
			Children =
			{
				new Label
				{
					Text = "Main Page - Tap entry, type text, then navigate",
					FontSize = 18
				},
				entry,
				navigateButton
			}
		};
	}
}

public class Issue34584_DestinationPage : ContentPage
{
	public Issue34584_DestinationPage()
	{
		Shell.SetNavBarIsVisible(this, false);

		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text = "Destination Page",
					FontSize = 24,
					AutomationId = "TargetLabel"
				}
			}
		};
	}
}
