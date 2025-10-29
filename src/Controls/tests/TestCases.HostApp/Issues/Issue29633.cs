namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29633, "[Android] Picker Does Not Show Selected Item Highlight", PlatformAffected.Android)]
public class Issue29633 : ContentPage
{
	public Issue29633()
	{
		var items = new List<string>
		{
			"Apple", "Banana", "Cherry", "Date", "Elderberry",
			"Fig", "Grape", "Honeydew", "Kiwi", "Lemon"
		};

		var picker = new Picker()
		{
			AutomationId = "HighlightPickerItem",
			ItemsSource = items,
			SelectedItem = items[6]
		};

		Content = new StackLayout
		{
			Children =
			{
				picker
			}
		};
	}
}