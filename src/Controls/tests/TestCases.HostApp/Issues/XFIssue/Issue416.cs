namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 416, "NavigationPage in PushModal does not show NavigationBar", PlatformAffected.Android)]
public class Issue416 : TestNavigationPage
{
	protected override void Init()
	{
		Navigation.PushAsync(new ContentPage
		{
			Title = "Test Page",
			Content = new Label
			{
				Text = "I should have a nav bar"
			}
		});
	}
}
