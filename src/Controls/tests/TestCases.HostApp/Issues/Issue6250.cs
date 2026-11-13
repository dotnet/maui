namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6250, "[Android] IsReadOnly=true EditText controls are not focusable", PlatformAffected.Android)]
public class Issue6250 : ContentPage
{
	public Issue6250()
	{
		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Entry { AutomationId = "EditableEntry", Placeholder = "Editable Entry" },
				new Entry { AutomationId = "ReadOnlyEntry", Text = "Read-only Entry", IsReadOnly = true },
				new Editor { AutomationId = "ReadOnlyEditor", Text = "Read-only Editor", IsReadOnly = true, HeightRequest = 100 },
				new SearchBar { AutomationId = "ReadOnlySearchBar", Text = "Read-only SearchBar", IsReadOnly = true },
			}
		};
	}
}
