namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34591, "Label with TailTruncation does not render text when initial text is null", PlatformAffected.iOS)]
public class Issue34591 : TestContentPage
{
	protected override void Init()
	{
		Label resultLabel = new Label
		{
			AutomationId = "ResultLabel",
			LineBreakMode = LineBreakMode.TailTruncation,
		};

		Button updateButton = new Button
		{
			AutomationId = "UpdateTextButton",
			Text = "Update Text"
		};

		updateButton.Clicked += (s, e) =>
		{
			resultLabel.Text = "Test";
		};

		Label instructions = new Label
		{
			Text = "Tap the 'Update Text' button. If the text 'Test' is visible in the label above this description, the test has passed."
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				updateButton,
				resultLabel,
				instructions
			}
		};
	}
}
