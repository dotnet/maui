namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23329, "Entry select all text on refocus does not work on WinUI", PlatformAffected.UWP)]
public class Issue23329 : ContentPage
{
	readonly Entry _textBox;

	public Issue23329()
	{
		_textBox = new Entry
		{
			AutomationId = "TextBox",
			WidthRequest = 300
		};
		_textBox.Focused += OnTextBoxFocused;

		var otherElement = new Button
		{
			AutomationId = "OtherElement",
			Text = "Other",
			WidthRequest = 128
		};

		Content = new VerticalStackLayout
		{
			Children = { _textBox, otherElement }
		};
	}

	void OnTextBoxFocused(object sender, FocusEventArgs e)
	{
		if (_textBox.Text != null)
		{
			_textBox.CursorPosition = 0;
			_textBox.SelectionLength = _textBox.Text.Length;
		}
	}
}
