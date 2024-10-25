namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2035, "App crashes when setting CurrentPage on TabbedPage in ctor in 2.5.1pre1", PlatformAffected.Android)]
	public class Issue2035 : TestTabbedPage
	{
		const string Success = "Success";
		protected override void Init()
		{
			Title = "Bug";
			Children.Add(new ContentPage() { Title = "Page 1" });
			Children.Add(new ContentPage() { Title = "Page 2", Content = new Label { AutomationId = Success, Text = Success } });
			Children.Add(new ContentPage() { Title = "Page 3" });
			CurrentPage = Children[1];
		}
	}
}