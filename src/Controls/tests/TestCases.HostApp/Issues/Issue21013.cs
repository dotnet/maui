namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21013, "OnKeyUp OnKeyDown dispatchKeyEvent not firing when back button pressed with keyboard open", PlatformAffected.Android)]
public class Issue21013 : ContentPage
{
	public Issue21013()
	{
		var entry = new Entry
		{
			Placeholder = "Tap to focus and show keyboard",
			AutomationId = "TestEntry"
		};

		var focusStatusLabel = new Label
		{
			Text = "IsFocused: false",
			AutomationId = "FocusStatusLabel"
		};

		entry.Focused += (s, e) =>
		{
			focusStatusLabel.Text = "IsFocused: true";
		};

		entry.Unfocused += (s, e) =>
		{
			focusStatusLabel.Text = "IsFocused: false";
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Label
				{
					Text = "1. Tap the entry to show keyboard\n2. Press the back button\n3. Entry should lose focus (ClearFocus called)"
				},
				entry,
				focusStatusLabel
			}
		};
	}
}
