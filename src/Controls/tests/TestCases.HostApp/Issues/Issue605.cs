namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 605, "Allow text selection and copy in readonly Entry", PlatformAffected.iOS)]
public class Issue605 : TestContentPage
{
	protected override void Init()
	{
		Content = new Entry()
		{
			Placeholder = "Enter text",
			Text = "sad",
			AutomationId = "Entry",
			TextColor = Colors.Black,
			IsReadOnly = true
		};
	}
}