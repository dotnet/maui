namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6323, "TabbedPage Page not watching icon changes", PlatformAffected.UWP)]
public class Issue6323 : TestTabbedPage
{
	protected override void Init()
	{
		SelectedTabColor = Colors.Purple;
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.StartTimer(TimeSpan.FromSeconds(2), () =>
		{
			Children.Add(new ContentPage
			{
				Content = new Label { Text = "Success", AutomationId = "Success" },
				Title = "I'm a title"
			});
			return false;
		});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
	}
}
