namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25585, "App Unresponsive when prompting the user from a new page", PlatformAffected.Android)]

public class Issue25585 : Shell
{
	public Issue25585()
	{
		this.FlyoutBehavior = FlyoutBehavior.Flyout;

		var shellContent = new ShellContent
		{
			Title = "Second Page",
			Route = "MainPage",
			Content = new Issue25585ContentPage()
		};

		Items.Add(new ShellContent
		{
			Title = "First Page",
			Route = "First",
			Content = new ContentPage
			{
				Content = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Text = "Go to SecondPage",
					AutomationId = "GoToSecondPage",
				}
			}
		});
		Items.Add(shellContent);
	}
}

public class Issue25585ContentPage : ContentPage
{
	public Issue25585ContentPage()
	{
		Content = new StackLayout
		{
			Children =
				{
					new Label { Text = "If DisplayAlert is shown, the test passed" },
				}
		};
		DisplayAlert("Hi", "This is from Constructor", "OK", "Cancel");
	}
}


