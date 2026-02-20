namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32885, "[iOS] Text Color of the Entry, TimePicker and SearchBar is not properly worked on dynamic Scenarios", PlatformAffected.iOS)]
public class Issue32885 : ContentPage
{
	Entry _entry;
	Button _button;
	public Issue32885()
	{
		_entry = new Entry
		{
			Text = "This is a test entry",
			AutomationId = "TestEntry",
			WidthRequest = 200,
			HeightRequest = 40
		};

		_button = new Button
		{
			Text = "Change Text Color dynamically to null",
			AutomationId = "TestButton",
			Command = new Command(() =>
			{
				_entry.TextColor = _entry.TextColor == null ? Colors.Green : null;
			})
		};

		Content = new VerticalStackLayout
		{
			Children = { _entry, _button }
		};
	}
}