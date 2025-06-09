namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29851, "[iOS] FormattedText with text color causes stack overflow", PlatformAffected.All)]
public class Issue29851 : TestContentPage
{
	protected override void Init()
	{
		Content = new Label
		{
			AutomationId = "label",
			TextColor = Colors.Green,
			FormattedText = new FormattedString
			{
				Spans =
				{
					new Span { Text = "Hello, World!" },
					new Span { Text = "*", TextColor = Colors.Red }
				}
			}
		};
		;
	}
}