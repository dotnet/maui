namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32825, "MauiDoneAccessoryView DoneButton should be accessible for UI testing", PlatformAffected.iOS)]
public class MauiDoneAccessoryButton : TestContentPage
{
	protected override void Init()
	{
		var entry = new Entry
		{
			AutomationId = "NumericEntry",
			Keyboard = Keyboard.Numeric,
			Placeholder = "Tap here to show numeric keyboard with Done button"
		};

		var label = new Label
		{
			AutomationId = "ResultLabel",
			Text = "Waiting for entry to be focused"
		};

		entry.Focused += (s, e) =>
		{
			label.Text = "Entry focused - Done button should be accessible";
		};

		entry.Completed += (s, e) =>
		{
			label.Text = "Entry completed - Done button was tapped";
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			Children =
			{
				new Label
				{
					Text = "This test verifies that the Done button on numeric keyboards has an accessibility identifier for UI testing.",
					Margin = new Thickness(0, 0, 0, 20)
				},
				entry,
				label
			}
		};
	}
}
