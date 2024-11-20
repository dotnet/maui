namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1650, "[macOS] Completed event of Entry raised on Tab key", PlatformAffected.macOS)]
public class GitHub1650 : TestContentPage
{
	Label _completedCountLabel = new Label
	{
		Text = "Completed count: 0",
		AutomationId = "CompletedCountLabel"
	};

	int _completedCount;
	public int CompletedCount
	{
		get { return _completedCount; }
		set
		{
			_completedCount = value;
			_completedCountLabel.Text = $"Completed count: {value}";
		}
	}

	protected override void Init()
	{
		// Setup our completed entry
		var entry = new Entry
		{
			Placeholder = "Press enter here!",
			AutomationId = "CompletedTargetEntry"
		};
		entry.Completed += (sender, e) =>
		{
			CompletedCount++;
		};

		StackLayout layout = new StackLayout();
		layout.Children.Add(_completedCountLabel);
		layout.Children.Add(entry);

		Content = layout;
	}
}
