namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 20911, "Updating text in the Entry does not update CursorPosition during the TextChanged event", PlatformAffected.iOS)]
public class Issue20911 : ContentPage
{
	Entry entry;
	Label cursorPositonStatusLabel;

	public Issue20911()
	{
		entry = new Entry
		{
			AutomationId = "ValidateEntryCursorPosition",
		};

		cursorPositonStatusLabel = new Label
		{
			AutomationId = "CursorPositionStatusLabel",
			Text = "Cursor Position: 0",
			FontSize = 16,
		};

		Button button = new Button
		{
			AutomationId = "ValidateEntryCursorPositionBtn",
			Text = "Validate Entry Cursor Position",
		};

		button.Clicked += (sender, e) =>
		{
			cursorPositonStatusLabel.Text = $"{entry.CursorPosition}";
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			Children = { entry, cursorPositonStatusLabel, button }
		};
	}
}