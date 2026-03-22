namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32583, "Shell Navigation Bar Remains Visible After Navigating Back to a ContentPage with a Hidden Navigation Bar", PlatformAffected.iOS)]
public class Issue32583 : TestShell
{
	protected override void Init()
	{
		ContentPage mainPage = new ContentPage
		{
			Title = "Main Page",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Label
					{
						AutomationId = "Issue32583DescriptionLabel",
						Text = "The test passes if the navigation bar remains hidden after navigating back from the Message Page",
					},
					new Button
					{
						Text = "Navigate to Message Page",
						AutomationId = "Issue32583NavigateButton",
						Command = new Command(async () =>
						{
							await Navigation.PushAsync(new Issue32583MessagePage());
						})
					}
				}
			}
		};

		Shell.SetNavBarIsVisible(mainPage, false);
		Shell.SetFlyoutBehavior(mainPage, FlyoutBehavior.Disabled);
		Shell.SetBackgroundColor(mainPage, Colors.LightGray);

		Items.Add(new ShellContent { Content = mainPage });
	}
}

public class Issue32583MessagePage : ContentPage
{
	public Issue32583MessagePage()
	{
		Shell.SetNavBarIsVisible(this, true);
		Shell.SetBackgroundColor(this, Colors.LightGray);
		Title = "Message Page";

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Button
				{
					Text = "Navigate Back to Main Page",
					AutomationId = "Issue32583BackButton",
					Command = new Command(async () =>
					{
						await Navigation.PopAsync();
					})
				}
			}
		};
	}
}