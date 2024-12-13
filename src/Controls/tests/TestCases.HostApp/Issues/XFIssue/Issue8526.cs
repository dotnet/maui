namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8526, "[Bug] DisplayPromptAsync hangs app, doesn't display when called in page load",
	PlatformAffected.All)]
public class Issue8526 : TestContentPage
{
	const string Success = "Success";

	protected override async void Init()
	{
		await DisplayPromptAsync(Success, "This prompt should display when the page loads.");
	}
}
