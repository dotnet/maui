namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39599, "Shell Navigation Bar Background Color Wrong", PlatformAffected.Android)]
public class Issue39599 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;
		Shell.SetBackgroundColor(this, Colors.Blue);
		Shell.SetTitleColor(this, Colors.White);

		AddContentPage(new ContentPage
		{
			Title = "Issue 39599",
			Content = new Label
			{
				Text = "The navigation bar should be blue.",
				AutomationId = "BackgroundColorLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		});
	}
}
