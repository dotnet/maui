namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32463, "Android direct test launch", PlatformAffected.Android)]
public class Issue32463 : ContentPage
{
	public Issue32463()
	{
		Content = new Label
		{
			Text = "Launched directly to the test",
			AutomationId = "DirectLaunchLabel"
		};
	}
}
