namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 28975, "Setting the Entry keyboard type to Date results in it behaving like a password entry", PlatformAffected.UWP)]
public class Issue28975 : ContentPage
{
	public Issue28975()
	{
		Entry dateEntry = new Entry
		{
			AutomationId = "DateEntry",
			Keyboard = Keyboard.Date,
			Placeholder = "Enter a date"
		};

		VerticalStackLayout verticalStackLayout = new VerticalStackLayout
		{
			Padding = 20,
			Children = { dateEntry }
		};

		Content = verticalStackLayout;
	}
}
