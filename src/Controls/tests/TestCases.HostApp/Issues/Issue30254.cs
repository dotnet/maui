namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30254, "(Windows) Shell.FlyoutBehavior=Flyout forces the title height space above the tab bar even if the page title is empty", PlatformAffected.UWP)]
public class Issue30254 : Shell
{
	public Issue30254()
	{
		BackgroundColor = Colors.Blue;
		FlyoutBehavior = FlyoutBehavior.Flyout;

		var goToWithTitleButton = new Button
		{
			AutomationId = "GoToWithTitleButton",
			Text = "Go to page with title",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		goToWithTitleButton.Clicked += (s, e) => GoToAsync("//WithTitleTab");

		var pageWithEmptyTitle = new ContentPage
		{
			Title = "",
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						AutomationId = "MainPageLabel",
						Text = "This page has an empty title.",
						HorizontalOptions = LayoutOptions.Center,
					},
					goToWithTitleButton
				}
			}
		};

		var goToEmptyTitleButton = new Button
		{
			AutomationId = "GoToEmptyTitleButton",
			Text = "Go to page with empty title",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		goToEmptyTitleButton.Clicked += (s, e) => GoToAsync("//EmptyTitleTab");

		var pageWithTitle = new ContentPage
		{
			Title = "Page With Title",
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						AutomationId = "WithTitleLabel",
						Text = "This page has a title, so header space is expected.",
						HorizontalOptions = LayoutOptions.Center,
					},
					goToEmptyTitleButton
				}
			}
		};

		Items.Add(new FlyoutItem
		{
			Title = "Header Test",
			Items =
			{
				new ShellContent { Title = "Empty Title", AutomationId = "EmptyTitleTab", Route = "EmptyTitleTab", Content = pageWithEmptyTitle },
				new ShellContent { Title = "With Title", AutomationId = "WithTitleTab", Route = "WithTitleTab", Content = pageWithTitle },
			}
		});
	}
}
