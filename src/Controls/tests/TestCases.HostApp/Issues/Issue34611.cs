namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34611, "Entry and Editor BackgroundColor not reset to Null", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34611 : TestContentPage
{
	Entry _entry;
	Editor _editor;

	protected override void Init()
	{
		Title = "Issue34611";

		_entry = new Entry
		{
			AutomationId = "TestEntry",
			Text = "Entry background should reset",
			Placeholder = "Entry"
		};

		_editor = new Editor
		{
			AutomationId = "TestEditor",
			Text = "Editor background should reset",
			HeightRequest = 120,
			Placeholder = "Editor"
		};

		var applyButton = new Button
		{
			AutomationId = "ApplyBackgroundColorButton",
			Text = "Apply BackgroundColor"
		};

		applyButton.Clicked += (_, _) =>
		{
			_entry.BackgroundColor = Colors.Red;
			_editor.BackgroundColor = Colors.LightBlue;
		};

		var resetButton = new Button
		{
			AutomationId = "ResetToDefaultButton",
			Text = "Reset to Default"
		};

		resetButton.Clicked += (_, _) =>
		{
			_entry.BackgroundColor = null;
			_editor.BackgroundColor = null;
		};

		Content = new VerticalStackLayout
		{
			Margin = new Thickness(20, 0, 20, 0),
			Spacing = 12,
			Children =
			{
				new Label
				{
					Text = "Apply custom backgrounds, then reset them to null."
				},
				_entry,
				_editor,
				applyButton,
				resetButton
			}
		};
	}
}
