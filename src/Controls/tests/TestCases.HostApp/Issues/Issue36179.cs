namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36179, "Entry.Completed accidentally fired on Enter key press in IME candidate window", PlatformAffected.Windows)]
public class Issue36179 : TestContentPage
{
	protected override void Init()
	{
		var entry = new Entry
		{
			AutomationId = "TestEntry",
			Placeholder = "Focus and press Enter"
		};

		var completedLabel = new Label
		{
			Text = "Completed: 0",
			AutomationId = "CompletedCountLabel"
		};

		int completedCount = 0;

		entry.Completed += (s, e) =>
		{
			completedCount++;
			completedLabel.Text = $"Completed: {completedCount}";
		};

		Content = new VerticalStackLayout
		{
			new Label
			{
				Text = "Test: IME candidate Enter should NOT fire Completed. Real Enter should fire Completed."
			},
			entry,
			completedLabel
		};
	}
}
