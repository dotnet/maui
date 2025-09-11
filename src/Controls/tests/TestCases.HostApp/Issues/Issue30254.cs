namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30254, "(Windows) Shell.FlyoutBehavior=Flyout forces the title height space above the tab bar even if the page title is empty", PlatformAffected.UWP)]
public class Issue30254 : Shell
{
	public Issue30254()
	{
		BackgroundColor = Colors.Blue;
		FlyoutBehavior = FlyoutBehavior.Flyout;

		var pageWithEmptyTitle = new ContentPage
		{
			Title = "",
			Content = new Button
			{
				AutomationId = "MainPageButton",
				Text = "This page has an empty title.",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			}
		};

		var pageWithTitle = new ContentPage
		{
			Title = "Page With Title",
			Content = new Label
			{
				Text = "This page has a title, so header space is expected.",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		Items.Add(new FlyoutItem
		{
			Title = "Header Test",
			Items =
			{
				new ShellContent { Title = "Empty Title", Content = pageWithEmptyTitle },
				new ShellContent { Title = "With Title", Content = pageWithTitle },
			}
		});
	}
}
