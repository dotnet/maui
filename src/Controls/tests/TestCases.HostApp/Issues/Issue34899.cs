namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34899, ".Net 10 Picker item not centered and wrong focus outline of Entry/Picker on Mac", PlatformAffected.macOS)]
public class Issue34899 : ContentPage
{
	public Issue34899()
	{
		var entry = new Entry
		{
			Placeholder = "Type something here",
			AutomationId = "Issue34899Entry"
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				entry
			}
		};
	}
}
