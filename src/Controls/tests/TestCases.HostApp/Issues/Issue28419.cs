namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28419, "SearchBar focus/unfocus do not fire on Windows", PlatformAffected.UWP)]
public class Issue28419 : ContentPage
{
	public Issue28419()
	{
		var label = new Label();
		var searchBar = new SearchBar { Placeholder = "SearchBar", AutomationId = "SearchBar" };
		var entry = new Entry { Placeholder = "Entry", AutomationId = "Entry" };

		searchBar.Focused += (sender, e) =>
		{
			label.Text = "SearchBar Focused";
		};

		searchBar.Unfocused += (sender, e) =>
		{
			label.Text = "SearchBar Unfocused";
		};

		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Children = { label, searchBar, entry }
		};
	}
}