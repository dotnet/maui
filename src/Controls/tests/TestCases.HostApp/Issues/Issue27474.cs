namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 27474, "Picker view items are not displayed when setting the title on the picker control", PlatformAffected.macOS)]
public class Issue27474 : ContentPage
{
	public Issue27474()
	{
		this.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		Content = new Picker
		{
			AutomationId = "Picker",
			ItemsSource = new List<string> { "Badminton", "Football", "Cricket", "Chess", "Swimming" },
			Title = "Select sports",
		};
	}
}